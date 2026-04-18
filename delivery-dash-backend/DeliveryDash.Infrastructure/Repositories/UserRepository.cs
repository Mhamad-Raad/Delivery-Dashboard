using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private const string USERS_CACHE_PREFIX = "users:filtered:";
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

        public UserRepository(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public async Task<(bool IsValid, User? User, string? ErrorMessage)> ValidateRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return (false, null, "Refresh token is required.");

            var user = await GetUserByRefreshTokenAsync(refreshToken);

            if (user == null)
                return (false, null, "Invalid refresh token.");

            if (user.RefreshTokenExpiresAtUtc == null)
                return (false, null, "Refresh token expiration is not set.");

            if (user.RefreshTokenExpiresAtUtc < DateTime.UtcNow)
                return (false, user, "Refresh token has expired.");

            return (true, user, null);
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetFilteredUsersAsync(
            int page,
            int limit,
            Role? role = null,
            string? searchTerm = null,
            string? buildingNameSearch = null)
        {
            // Generate cache key based on all parameters
            var cacheKey = GenerateCacheKey(page, limit, role, searchTerm, buildingNameSearch);

            // Try to get from cache first
            var cachedResult = await _cacheService.GetAsync<CachedUserResult>(cacheKey);
            if (cachedResult != null)
            {
                return (cachedResult.Users, cachedResult.TotalCount);
            }

            // If not in cache, query database
            var query = _context.Users.AsNoTracking().AsQueryable();

            // Filter by role
            if (role.HasValue)
            {
                var roleName = GetRoleNameFromEnum(role.Value);
                var normalizedRole = roleName.ToUpper();

                query = query.Where(u =>
                    _context.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(_context.Roles,
                            ur => ur.RoleId,
                            r => r.Id,
                            (ur, r) => r.NormalizedName)
                        .Any(rn => rn == normalizedRole)
                );
            }

            // Filter by building name search
            if (!string.IsNullOrWhiteSpace(buildingNameSearch))
            {
                var escapedBuildingName = EscapeLikePattern(buildingNameSearch.Trim());
                var buildingSearchPattern = $"%{escapedBuildingName}%";

                query = query.Where(u =>
                    _context.Addresses
                        .Where(a => a.UserId == u.Id && a.BuildingId != null)
                        .Join(
                            _context.Buildings,
                            a => a.BuildingId.Value,
                            b => b.Id,
                            (a, b) => b.Name)
                        .Any(name => EF.Functions.ILike(name, buildingSearchPattern))
                );
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var escapedTerm = EscapeLikePattern(searchTerm.Trim());
                var searchPattern = $"%{escapedTerm}%";

                var parts = escapedTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)
                {
                    var firstPart = $"%{parts[0]}%";
                    var secondPart = $"%{parts[1]}%";

                    query = query.Where(u =>
                        (u.Email != null && EF.Functions.ILike(u.Email, searchPattern)) ||
                        EF.Functions.ILike(u.FirstName, searchPattern) ||
                        EF.Functions.ILike(u.LastName, searchPattern) ||
                        (u.PhoneNumber != null && EF.Functions.ILike(u.PhoneNumber, searchPattern)) ||
                        (EF.Functions.ILike(u.FirstName, firstPart) && EF.Functions.ILike(u.LastName, secondPart)) ||
                        (EF.Functions.ILike(u.FirstName, secondPart) && EF.Functions.ILike(u.LastName, firstPart))
                    );
                }
                else
                {
                    query = query.Where(u =>
                        (u.Email != null && EF.Functions.ILike(u.Email, searchPattern)) ||
                        EF.Functions.ILike(u.FirstName, searchPattern) ||
                        EF.Functions.ILike(u.LastName, searchPattern) ||
                        (u.PhoneNumber != null && EF.Functions.ILike(u.PhoneNumber, searchPattern))
                    );
                }
            }

            query = query.OrderBy(u => u.Email).ThenBy(u => u.Id);

            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            // Store result in cache
            var resultToCache = new CachedUserResult
            {
                Users = users,
                TotalCount = totalCount
            };
            await _cacheService.SetAsync(cacheKey, resultToCache, CacheExpiration);

            return (users, totalCount);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Invalidate all filtered user caches when a user is updated
            await InvalidateFilteredUsersCache();
        }

        public async Task DeleteAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                
                // Invalidate cache after successful deletion
                await InvalidateFilteredUsersCache();
            }
        }

        public async Task<Dictionary<Guid, string>> GetUserRolesDictionaryAsync(IEnumerable<Guid> userIds)
        {
            var userIdsList = userIds as List<Guid> ?? userIds.ToList();

            if (!userIdsList.Any())
            {
                return new Dictionary<Guid, string>();
            }

            // Fetch user-role mappings with role names in a single query
            var userRoles = await _context.UserRoles
                .AsNoTracking()
                .Where(ur => userIdsList.Contains(ur.UserId))
                .Join(
                    _context.Roles.AsNoTracking(),
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, RoleName = r.Name })
                .ToDictionaryAsync(x => x.UserId, x => x.RoleName!);

            return userRoles;
        }

        /// <summary>
        /// Invalidates all cached filtered user queries
        /// Call this whenever user data changes (create, update, delete)
        /// </summary>
        public async Task InvalidateFilteredUsersCache()
        {
            await _cacheService.RemoveByPatternAsync($"{USERS_CACHE_PREFIX}*");
        }

        private static string GenerateCacheKey(
            int page,
            int limit,
            Role? role,
            string? searchTerm,
            string? buildingNameSearch)
        {
            var roleStr = role.HasValue ? role.Value.ToString() : "null";
            var searchStr = string.IsNullOrWhiteSpace(searchTerm) ? "null" : searchTerm.Trim();
            var buildingStr = string.IsNullOrWhiteSpace(buildingNameSearch) ? "null" : buildingNameSearch.Trim();

            return $"{USERS_CACHE_PREFIX}p{page}:l{limit}:r{roleStr}:s{searchStr}:b{buildingStr}";
        }

        private static string EscapeLikePattern(string pattern)
        {
            return pattern
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
        }

        private static string GetRoleNameFromEnum(Role role)
        {
            return role switch
            {
                Role.SuperAdmin => "SuperAdmin",
                Role.Admin => "Admin",
                Role.Vendor => "Vendor",
                Role.Tenant => "Tenant",
                Role.Driver => "VendorDriver",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Invalid role provided"),
            };
        }

        // Helper class for caching
        private class CachedUserResult
        {
            public List<User> Users { get; set; } = new();
            public int TotalCount { get; set; }
        }
    }
}