using DeliveryDash.Application.Requests.VendorStaffRequests;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.VendorStaffResponses;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IVendorStaffService
    {
        Task<VendorStaffResponse> GetByIdAsync(int id);
        Task<VendorStaffResponse?> GetByUserIdAsync(Guid userId);
        Task<PagedResponse<VendorStaffResponse>> GetStaffByVendorIdAsync(
            int vendorId,
            int page = 1,
            int limit = 10,
            bool? isActive = null,
            string? searchTerm = null);
        Task<VendorStaffResponse> CreateAsync(CreateVendorStaffRequest request, int vendorId);
        Task<VendorStaffResponse> UpdateAsync(int id, UpdateVendorStaffRequest request);
        Task DeactivateAsync(int id);
        Task DeleteAsync(int id);
        Task<int?> GetVendorIdForStaffUserAsync(Guid userId);
    }
}