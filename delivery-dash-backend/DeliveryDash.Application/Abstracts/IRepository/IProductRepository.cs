using DeliveryDash.Domain.Entities;

namespace DeliveryDash.Application.Abstracts.IRepository
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<(IEnumerable<Product> Products, int Total)> GetProductsPagedAsync(
            int page,
            int limit,
            int? vendorId = null,
            int? categoryId = null,
            string? searchName = null,
            bool? inStock = null);
        Task<IEnumerable<Product>> GetProductsByVendorIdAsync(int vendorId);
        Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}