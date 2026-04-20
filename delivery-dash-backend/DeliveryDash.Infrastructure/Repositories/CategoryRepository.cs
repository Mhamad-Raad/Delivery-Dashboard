using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Vendor)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetByVendorIdAsync(int vendorId)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.VendorId == vendorId)
                .OrderBy(c => c.SortOrder ?? int.MaxValue)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Category> Categories, int Total)> GetPagedAsync(
            int page,
            int limit,
            int? vendorId = null,
            string? searchName = null)
        {
            var query = _context.Categories
                .Include(c => c.Vendor)
                .AsNoTracking()
                .AsQueryable();

            if (vendorId.HasValue)
            {
                query = query.Where(c => c.VendorId == vendorId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                searchName = searchName.Trim();
                query = query.Where(c => EF.Functions.ILike(c.Name, $"%{searchName}%"));
            }

            var total = await query.CountAsync();

            var categories = await query
                .OrderBy(c => c.VendorId)
                .ThenBy(c => c.SortOrder ?? int.MaxValue)
                .ThenBy(c => c.Name)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (categories, total);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                // Null out CategoryId on any products that reference this category
                await _context.Products
                    .Where(p => p.CategoryId == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CategoryId, (int?)null));

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByNameForVendorAsync(int vendorId, string name)
        {
            return await _context.Categories
                .AnyAsync(c => c.VendorId == vendorId && EF.Functions.ILike(c.Name, name));
        }

        public async Task<int> CountByVendorIdAsync(int vendorId)
        {
            return await _context.Categories
                .CountAsync(c => c.VendorId == vendorId);
        }

        public async Task<int> CountProductsAsync(int categoryId)
        {
            return await _context.Products
                .CountAsync(p => p.CategoryId == categoryId);
        }

        public async Task<Dictionary<int, int>> CountProductsByCategoryIdsAsync(IEnumerable<int> categoryIds)
        {
            var ids = categoryIds.ToList();
            return await _context.Products
                .Where(p => p.CategoryId.HasValue && ids.Contains(p.CategoryId.Value))
                .GroupBy(p => p.CategoryId!.Value)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);
        }
    }
}
