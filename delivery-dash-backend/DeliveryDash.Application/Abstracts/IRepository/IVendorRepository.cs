using DeliveryDash.Domain.Entities;

namespace DeliveryDash.Application.Abstracts.IRepository
{
    public interface IVendorRepository
    {
        Task<Vendor?> GetByIdAsync(int id);
        Task<Vendor?> GetByUserIdAsync(Guid userId);
        Task<(IEnumerable<Vendor> Vendors, int Total)> GetVendorsPagedAsync(
            int page,
            int limit,
            string? searchName = null,
            int? vendorCategoryId = null);
        Task<Vendor> CreateAsync(Vendor vendor);
        Task<Vendor> UpdateAsync(Vendor vendor);
        Task DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> UserHasVendorAsync(Guid userId);
        Task<int> CountByVendorCategoryIdAsync(int vendorCategoryId);
    }
}
