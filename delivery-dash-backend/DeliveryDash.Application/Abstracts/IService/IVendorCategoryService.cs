using DeliveryDash.Application.Requests.VendorCategoryRequests;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.VendorCategoryResponses;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IVendorCategoryService
    {
        Task<VendorCategoryResponse> GetByIdAsync(int id);
        Task<IEnumerable<VendorCategoryResponse>> GetAllAsync(bool activeOnly = false);
        Task<PagedResponse<VendorCategoryResponse>> GetPagedAsync(
            int page,
            int limit,
            string? searchName = null,
            bool activeOnly = false);
        Task<VendorCategoryResponse> CreateAsync(CreateVendorCategoryRequest request);
        Task<VendorCategoryResponse> UpdateAsync(int id, UpdateVendorCategoryRequest request);
        Task DeleteAsync(int id);
    }
}
