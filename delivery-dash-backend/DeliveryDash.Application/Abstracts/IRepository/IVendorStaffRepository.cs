using DeliveryDash.Domain.Entities;

namespace DeliveryDash.Application.Abstracts.IRepository
{
    public interface IVendorStaffRepository
    {
        Task<VendorStaff?> GetByIdAsync(int id);
        Task<VendorStaff?> GetByUserIdAsync(Guid userId);
        Task<VendorStaff?> GetByUserIdAndVendorIdAsync(Guid userId, int vendorId);
        Task<(IEnumerable<VendorStaff> Staff, int Total)> GetStaffByVendorIdPagedAsync(
            int vendorId,
            int page,
            int limit,
            bool? isActive = null,
            string? searchTerm = null);
        Task<IEnumerable<VendorStaff>> GetActiveStaffByVendorIdAsync(int vendorId);
        Task<VendorStaff> CreateAsync(VendorStaff vendorStaff);
        Task<VendorStaff> UpdateAsync(VendorStaff vendorStaff);
        Task DeleteAsync(int id);
        Task<bool> IsUserStaffOfVendorAsync(Guid userId, int vendorId);
        Task<bool> UserHasStaffAssignmentAsync(Guid userId);
    }
}