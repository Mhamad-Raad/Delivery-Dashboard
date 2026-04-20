using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Infrastructure.Repositories
{
    public class VendorCategoryRepository : IVendorCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public VendorCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VendorCategory?> GetByIdAsync(int id)
        {
            return await _context.VendorCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(vc => vc.Id == id);
        }

        public async Task<IEnumerable<VendorCategory>> GetAllAsync(bool activeOnly = false)
        {
            var query = _context.VendorCategories.AsNoTracking().AsQueryable();
            if (activeOnly)
                query = query.Where(vc => vc.IsActive);

            return await query.OrderBy(vc => vc.Name).ToListAsync();
        }

        public async Task<(IEnumerable<VendorCategory> Items, int Total)> GetPagedAsync(
            int page,
            int limit,
            string? searchName = null,
            bool activeOnly = false)
        {
            var query = _context.VendorCategories.AsNoTracking().AsQueryable();

            if (activeOnly)
                query = query.Where(vc => vc.IsActive);

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                searchName = searchName.Trim();
                query = query.Where(vc => EF.Functions.ILike(vc.Name, $"%{searchName}%"));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(vc => vc.Name)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (items, total);
        }

        public async Task<VendorCategory> CreateAsync(VendorCategory category)
        {
            await _context.VendorCategories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<VendorCategory> UpdateAsync(VendorCategory category)
        {
            _context.VendorCategories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.VendorCategories.FindAsync(id);
            if (category != null)
            {
                _context.VendorCategories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var query = _context.VendorCategories.Where(vc => EF.Functions.ILike(vc.Name, name));
            if (excludeId.HasValue)
                query = query.Where(vc => vc.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<Dictionary<int, int>> CountVendorsByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            return await _context.Vendors
                .Where(v => idList.Contains(v.VendorCategoryId))
                .GroupBy(v => v.VendorCategoryId)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count);
        }
    }
}
