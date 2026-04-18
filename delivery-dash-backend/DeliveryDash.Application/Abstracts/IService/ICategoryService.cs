using DeliveryDash.Application.Requests.CategoryRequests;
using DeliveryDash.Application.Responses.CategoryResponses;
using DeliveryDash.Application.Responses.Common;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface ICategoryService
    {
        Task<CategoryDetailResponse> GetCategoryByIdAsync(int id);
        Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
        Task<IEnumerable<CategoryResponse>> GetTopLevelCategoriesAsync();
        Task<IEnumerable<CategoryResponse>> GetSubCategoriesAsync(int parentCategoryId);
        Task<PagedResponse<CategoryResponse>> GetCategoriesPagedAsync(
            int page = 1,
            int limit = 10,
            string? searchName = null,
            int? parentCategoryId = null);
        Task<CategoryDetailResponse> CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryDetailResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
        Task DeleteCategoryAsync(int id);
    }
}