using MalDash.Application.Requests.VendorRequests;
using MalDash.Application.Responses.Common;
using MalDash.Application.Responses.VendorResponses;
using MalDash.Domain.Enums;

namespace MalDash.Application.Abstracts.IService
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