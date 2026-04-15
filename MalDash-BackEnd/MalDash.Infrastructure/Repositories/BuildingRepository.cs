using MalDash.Application.Abstracts.IRepository;
using MalDash.Application.Abstracts.IService;
using MalDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MalDash.Infrastructure.Repositories
{
    public class BuildingRepository : IBuildingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private const string BUILDINGS_CACHE_PREFIX = "buildings:filtered:";
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

        public BuildingRepository(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<Building?> GetByIdAsync(int id)
        {
            return await _context.Buildings
                .Include(b => b.Floors.OrderBy(f => f.FloorNumber))
                    .ThenInclude(f => f.Apartments.OrderBy(a => a.ApartmentName))
                        .ThenInclude(a => a.Addresses)
                            .ThenInclude(addr => addr.User)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Building?> GetByNameAsync(string name)
        {
            return await _context.Buildings
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Apartments)
                .Include(b => b.Addresses)
                .FirstOrDefaultAsync(b => b.Name == name);
        }

        public async Task<(IEnumerable<Building> Buildings, int Total)> GetBuildingsPagedAsync(
            int page,
            int limit,
            string? searchName = null)
        {
            // Only cache when there's NO search term (high cache hit rate)
            // Search terms are too variable to cache effectively
            if (string.IsNullOrWhiteSpace(searchName))
            {
                var cacheKey = GenerateBuildingCacheKey(page, limit);
                var cachedResult = await _cacheService.GetAsync<CachedBuildingResult>(cacheKey);

                if (cachedResult != null)
                {
                    return (cachedResult.Buildings, cachedResult.Total);
                }
            }

            var query = _context.Buildings
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Apartments)
                .Include(b => b.Addresses)
                .AsNoTracking()
                .AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var escapedName = EscapeLikePattern(searchName.Trim());
                query = query.Where(b =>
                    EF.Functions.ILike(b.Name, $"%{escapedName}%"));
            }

            // Get total count before pagination
            var total = await query.CountAsync();

            // Apply pagination
            var buildings = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            // Only cache if no search term was used
            if (string.IsNullOrWhiteSpace(searchName))
            {
                var resultToCache = new CachedBuildingResult
                {
                    Buildings = buildings,
                    Total = total
                };
                var cacheKey = GenerateBuildingCacheKey(page, limit);
                await _cacheService.SetAsync(cacheKey, resultToCache, CacheExpiration);
            }

            return (buildings, total);
        }

        public async Task<Building> CreateAsync(Building building)
        {
            await _context.Buildings.AddAsync(building);
            await _context.SaveChangesAsync();

            // Invalidate cache after creating a building
            await InvalidateBuildingsCache();

            return building;
        }

        public async Task<Building> UpdateAsync(Building building)
        {
            _context.Buildings.Update(building);
            await _context.SaveChangesAsync();

            // Invalidate cache after updating a building
            await InvalidateBuildingsCache();

            return building;
        }

        public async Task DeleteAsync(int id)
        {
            var building = await GetByIdAsync(id);
            if (building != null)
            {
                _context.Buildings.Remove(building);
                await _context.SaveChangesAsync();

                // Invalidate cache after deleting a building
                await InvalidateBuildingsCache();
            }
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Buildings.AnyAsync(b => b.Name == name);
        }

        public async Task<(Building? Building, bool HasOccupants)> GetByFloorIdAsync(int floorId)
        {
            // Get minimal floor info with occupancy check - no loading apartments into memory
            var floorInfo = await _context.Floors
                .AsNoTracking()
                .Where(f => f.Id == floorId)
                .Select(f => new
                {
                    f.BuildingId,
                    f.FloorNumber,
                    // Check for occupants at DB level using EXISTS (O(1))
                    HasOccupants = f.Apartments.Any(a => a.Addresses.Any(addr => addr.UserId.HasValue))
                })
                .FirstOrDefaultAsync();

            if (floorInfo == null)
                return (null, false);

            // Get the building with only floors that need floor number adjustment
            var building = await _context.Buildings
                .Include(b => b.Floors.Where(f => f.FloorNumber >= floorInfo.FloorNumber))
                .FirstOrDefaultAsync(b => b.Id == floorInfo.BuildingId);

            return (building, floorInfo.HasOccupants);
        }

        public async Task<Building?> GetByFloorIdForApartmentAsync(int floorId)
        {
            // Get floor info to find building
            var floorInfo = await _context.Floors
                .AsNoTracking()
                .Where(f => f.Id == floorId)
                .Select(f => new { f.BuildingId })
                .FirstOrDefaultAsync();

            if (floorInfo == null)
                return null;

            // Get building with all floors and apartments to calculate next apartment number
            return await _context.Buildings
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Apartments)
                .FirstOrDefaultAsync(b => b.Id == floorInfo.BuildingId);
        }

        public async Task<Building?> GetByApartmentIdAsync(int apartmentId)
        {
            // Get apartment info to find floor and building
            var apartmentInfo = await _context.Apartments
                .AsNoTracking()
                .Where(a => a.Id == apartmentId)
                .Select(a => new { a.FloorId })
                .FirstOrDefaultAsync();

            if (apartmentInfo == null)
                return null;

            // Get floor info to find building
            var floorInfo = await _context.Floors
                .AsNoTracking()
                .Where(f => f.Id == apartmentInfo.FloorId)
                .Select(f => new { f.BuildingId, f.Id })
                .FirstOrDefaultAsync();

            if (floorInfo == null)
                return null;

            // Get building with the specific floor and its apartments
            return await _context.Buildings
                .Include(b => b.Floors.Where(f => f.Id == floorInfo.Id))
                    .ThenInclude(f => f.Apartments)
                .FirstOrDefaultAsync(b => b.Id == floorInfo.BuildingId);
        }

        public async Task<(Building? Building, bool HasOccupant)> GetByApartmentIdForDeleteAsync(int apartmentId)
        {
            // Get apartment info with occupancy check
            var apartmentInfo = await _context.Apartments
                .AsNoTracking()
                .Where(a => a.Id == apartmentId)
                .Select(a => new
                {
                    a.FloorId,
                    HasOccupant = a.Addresses.Any(addr => addr.UserId.HasValue)
                })
                .FirstOrDefaultAsync();

            if (apartmentInfo == null)
                return (null, false);

            // Get floor info to find building
            var floorInfo = await _context.Floors
                .AsNoTracking()
                .Where(f => f.Id == apartmentInfo.FloorId)
                .Select(f => new { f.BuildingId, f.Id })
                .FirstOrDefaultAsync();

            if (floorInfo == null)
                return (null, false);

            // Get building with the specific floor and its apartments
            var building = await _context.Buildings
                .Include(b => b.Floors.Where(f => f.Id == floorInfo.Id))
                    .ThenInclude(f => f.Apartments)
                .FirstOrDefaultAsync(b => b.Id == floorInfo.BuildingId);

            return (building, apartmentInfo.HasOccupant);
        }

        /// <summary>
        /// Invalidates all cached building queries
        /// Call this whenever building data changes (create, update, delete)
        /// </summary>
        private async Task InvalidateBuildingsCache()
        {
            await _cacheService.RemoveByPatternAsync($"{BUILDINGS_CACHE_PREFIX}*");
        }

        private static string GenerateBuildingCacheKey(int page, int limit)
        {
            return $"{BUILDINGS_CACHE_PREFIX}p{page}:l{limit}";
        }

        private static string EscapeLikePattern(string pattern)
        {
            return pattern
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
        }

        // Helper class for caching
        private class CachedBuildingResult
        {
            public List<Building> Buildings { get; set; } = new();
            public int Total { get; set; }
        }
    }
}