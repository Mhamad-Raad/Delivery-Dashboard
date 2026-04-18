using DeliveryDash.Application.Requests.VendorRequests;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.VendorResponses;
using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IVendorService
    {
        Task<VendorDetailResponse> GetVendorByIdAsync(int id);
        Task<VendorDetailResponse?> GetVendorByUserIdAsync(Guid userId);
        Task<PagedResponse<VendorResponse>> GetAllVendorsAsync(
            int page = 1,
            int limit = 10,
            string? searchName = null,
            VendorType? type = null);
        Task<VendorDetailResponse> CreateVendorAsync(CreateVendorRequest request, string? imageUrl = null);
        Task<VendorDetailResponse> UpdateVendorAsync(int id, UpdateVendorRequest request, string? imageUrl = null);
        Task DeleteVendorAsync(int id);
    }
}