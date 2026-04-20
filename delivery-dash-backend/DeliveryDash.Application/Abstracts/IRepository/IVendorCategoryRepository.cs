using DeliveryDash.Domain.Entities;

namespace DeliveryDash.Application.Abstracts.IRepository
{
    public interface IVendorCategoryRepository
    {
        Task<VendorCategory?> GetByIdAsync(int id);
        Task<IEnumerable<VendorCategory>> GetAllAsync(bool activeOnly = false);
        Task<(IEnumerable<VendorCategory> Items, int Total)> GetPagedAsync(
            int page,
            int limit,
            string? searchName = null,
            bool activeOnly = false);
        Task<VendorCategory> CreateAsync(VendorCategory category);
        Task<VendorCategory> UpdateAsync(VendorCategory category);
        Task DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
        Task<Dictionary<int, int>> CountVendorsByIdsAsync(IEnumerable<int> ids);
    }
}
