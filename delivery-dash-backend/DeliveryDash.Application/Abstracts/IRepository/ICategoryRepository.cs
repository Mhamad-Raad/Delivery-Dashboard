using DeliveryDash.Domain.Entities;

namespace DeliveryDash.Application.Abstracts.IRepository
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetByVendorIdAsync(int vendorId);
        Task<(IEnumerable<Category> Categories, int Total)> GetPagedAsync(
            int page,
            int limit,
            int? vendorId = null,
            string? searchName = null);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<bool> ExistsByNameForVendorAsync(int vendorId, string name);
        Task<int> CountByVendorIdAsync(int vendorId);
        Task<int> CountProductsAsync(int categoryId);
        Task<Dictionary<int, int>> CountProductsByCategoryIdsAsync(IEnumerable<int> categoryIds);
    }
}
