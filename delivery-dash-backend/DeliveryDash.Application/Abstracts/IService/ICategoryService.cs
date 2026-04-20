using DeliveryDash.Application.Requests.CategoryRequests;
using DeliveryDash.Application.Responses.CategoryResponses;
using DeliveryDash.Application.Responses.Common;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface ICategoryService
    {
        Task<CategoryDetailResponse> GetByIdAsync(int id, int? requestingVendorId);
        Task<IEnumerable<CategoryResponse>> GetByVendorIdAsync(int vendorId);
        Task<IEnumerable<CategoryResponse>> GetMineAsync(int vendorId);
        Task<PagedResponse<CategoryResponse>> GetPagedAsync(
            int page,
            int limit,
            int? vendorId,
            string? searchName);
        Task<CategoryDetailResponse> CreateAsync(int vendorId, CreateCategoryRequest request);
        Task<CategoryDetailResponse> UpdateAsync(int id, int vendorId, UpdateCategoryRequest request);
        Task DeleteAsync(int id, int vendorId);
    }
}
