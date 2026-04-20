using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Infrastructure.Repositories
{
    public class VendorRepository : IVendorRepository
    {
        private readonly ApplicationDbContext _context;

        public VendorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Vendor?> GetByIdAsync(int id)
        {
            return await _context.Vendors
                .Include(v => v.User)
                .Include(v => v.VendorCategory)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Vendor?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Vendors
                .Include(v => v.User)
                .Include(v => v.VendorCategory)
                .FirstOrDefaultAsync(v => v.UserId == userId);
        }

        public async Task<(IEnumerable<Vendor> Vendors, int Total)> GetVendorsPagedAsync(
            int page,
            int limit,
            string? searchName = null,
            int? vendorCategoryId = null)
        {
            var query = _context.Vendors
                .Include(v => v.User)
                .Include(v => v.VendorCategory)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                searchName = searchName.Trim();
                query = query.Where(v => EF.Functions.ILike(v.Name, $"%{searchName}%"));
            }

            if (vendorCategoryId.HasValue)
            {
                query = query.Where(v => v.VendorCategoryId == vendorCategoryId.Value);
            }

            var total = await query.CountAsync();

            var vendors = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (vendors, total);
        }

        public async Task<Vendor> CreateAsync(Vendor vendor)
        {
            await _context.Vendors.AddAsync(vendor);
            await _context.SaveChangesAsync();
            return vendor;
        }

        public async Task<Vendor> UpdateAsync(Vendor vendor)
        {
            _context.Vendors.Update(vendor);
            await _context.SaveChangesAsync();
            return vendor;
        }

        public async Task DeleteAsync(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor != null)
            {
                _context.Vendors.Remove(vendor);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Vendors
                .AnyAsync(v => EF.Functions.ILike(v.Name, name));
        }

        public async Task<bool> UserHasVendorAsync(Guid userId)
        {
            return await _context.Vendors
                .AnyAsync(v => v.UserId == userId);
        }

        public async Task<int> CountByVendorCategoryIdAsync(int vendorCategoryId)
        {
            return await _context.Vendors
                .CountAsync(v => v.VendorCategoryId == vendorCategoryId);
        }
    }
}
