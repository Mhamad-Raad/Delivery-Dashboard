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

        public async Task<Catagory?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Catagory?> GetByNameAsync(string name)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => EF.Functions.ILike(c.Name, name));
        }

        public async Task<IEnumerable<Catagory>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Catagory>> GetTopLevelCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .AsNoTracking()
                .Where(c => c.ParentCategoryId == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Catagory>> GetSubCategoriesAsync(int parentCategoryId)
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .AsNoTracking()
                .Where(c => c.ParentCategoryId == parentCategoryId)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Catagory> Categories, int Total)> GetCategoriesPagedAsync(
            int page,
            int limit,
            string? searchName = null,
            int? parentCategoryId = null)
        {
            var query = _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                searchName = searchName.Trim();
                query = query.Where(c => EF.Functions.ILike(c.Name, searchName));
            }

            if (parentCategoryId.HasValue)
            {
                query = query.Where(c => c.ParentCategoryId == parentCategoryId.Value);
            }

            var total = await query.CountAsync();

            var categories = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (categories, total);
        }

        public async Task<Catagory> CreateAsync(Catagory category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Catagory> UpdateAsync(Catagory category)
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
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Categories
                .AnyAsync(c => EF.Functions.ILike(c.Name, name));
        }

        public async Task<bool> HasSubCategoriesAsync(int categoryId)
        {
            return await _context.Categories
                .AnyAsync(c => c.ParentCategoryId == categoryId);
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products
                .AnyAsync(p => p.CategoryId == categoryId);
        }
    }
}