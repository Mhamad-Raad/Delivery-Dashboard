using DeliveryDash.Application.Requests.ProductRequests;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.ProductResponses;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IProductService
    {
        Task<ProductDetailResponse> GetProductByIdAsync(int id);
        Task<PagedResponse<ProductResponse>> GetAllProductsAsync(
            int page = 1,
            int limit = 10,
            int? vendorId = null,
            int? categoryId = null,
            string? searchName = null,
            bool? inStock = null);
        Task<IEnumerable<ProductDetailResponse>> GetProductsByVendorIdAsync(int vendorId);
        Task<IEnumerable<ProductResponse>> GetProductsByCategoryIdAsync(int categoryId);
        Task<ProductDetailResponse> CreateProductAsync(CreateProductRequest request, int vendorId, string? imageUrl = null);
        Task<ProductDetailResponse> UpdateProductAsync(int id, UpdateProductRequest request, string? imageUrl = null);
        Task DeleteProductAsync(int id);
    }
}