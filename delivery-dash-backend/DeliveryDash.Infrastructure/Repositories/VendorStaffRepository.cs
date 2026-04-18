using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Infrastructure.Repositories
{
    public class VendorStaffRepository : IVendorStaffRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public VendorStaffRepository(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<VendorStaff?> GetByIdAsync(int id)
        {
            return await _context.VendorStaff
                .Include(vs => vs.User)
                .Include(vs => vs.Vendor)
                .FirstOrDefaultAsync(vs => vs.Id == id);
        }

        public async Task<VendorStaff?> GetByUserIdAsync(Guid userId)
        {
            return await _context.VendorStaff
                .Include(vs => vs.User)
                .Include(vs => vs.Vendor)
                .FirstOrDefaultAsync(vs => vs.UserId == userId && vs.IsActive);
        }

        public async Task<VendorStaff?> GetByUserIdAndVendorIdAsync(Guid userId, int vendorId)
        {
            return await _context.VendorStaff
                .Include(vs => vs.User)
                .Include(vs => vs.Vendor)
                .FirstOrDefaultAsync(vs => vs.UserId == userId && vs.VendorId == vendorId);
        }

        public async Task<(IEnumerable<VendorStaff> Staff, int Total)> GetStaffByVendorIdPagedAsync(
            int vendorId,
            int page,
            int limit,
            bool? isActive = null,
            string? searchTerm = null)
        {
            var query = _context.VendorStaff
                .Include(vs => vs.User)
                .Include(vs => vs.Vendor)
                .AsNoTracking()
                .Where(vs => vs.VendorId == vendorId);

            if (isActive.HasValue)
            {
                query = query.Where(vs => vs.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(vs =>
                    EF.Functions.ILike(vs.User.FirstName, $"%{searchTerm}%") ||
                    EF.Functions.ILike(vs.User.LastName, $"%{searchTerm}%") ||
                    EF.Functions.ILike(vs.User.Email!, $"%{searchTerm}%"));
            }

            var total = await query.CountAsync();

            var staff = await query
                .OrderByDescending(vs => vs.AssignedDate)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (staff, total);
        }

        public async Task<IEnumerable<VendorStaff>> GetActiveStaffByVendorIdAsync(int vendorId)
        {
            return await _context.VendorStaff
                .Include(vs => vs.User)
                .AsNoTracking()
                .Where(vs => vs.VendorId == vendorId && vs.IsActive)
                .ToListAsync();
        }

        public async Task<VendorStaff> CreateAsync(VendorStaff vendorStaff)
        {
            await _context.VendorStaff.AddAsync(vendorStaff);
            await _context.SaveChangesAsync();
            return vendorStaff;
        }

        public async Task<VendorStaff> UpdateAsync(VendorStaff vendorStaff)
        {
            _context.VendorStaff.Update(vendorStaff);
            await _context.SaveChangesAsync();
            return vendorStaff;
        }

        public async Task DeleteAsync(int id)
        {
            var staff = await _context.VendorStaff.FindAsync(id);
            if (staff != null)
            {
                _context.VendorStaff.Remove(staff);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsUserStaffOfVendorAsync(Guid userId, int vendorId)
        {
            return await _context.VendorStaff
                .AnyAsync(vs => vs.UserId == userId && vs.VendorId == vendorId && vs.IsActive);
        }

        public async Task<bool> UserHasStaffAssignmentAsync(Guid userId)
        {
            return await _context.VendorStaff
                .AnyAsync(vs => vs.UserId == userId);
        }
    }
}