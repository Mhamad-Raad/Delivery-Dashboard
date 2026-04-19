using DeliveryDash.Application.Requests.AddressRequests;
using DeliveryDash.Application.Responses.AddressResponses;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IAddressService
    {
        Task<List<AddressResponse>> GetMyAddressesAsync();
        Task<AddressResponse> GetByIdAsync(int id);
        Task<AddressResponse> CreateAsync(CreateAddressRequest request);
        Task<AddressResponse> UpdateAsync(int id, UpdateAddressRequest request);
        Task DeleteAsync(int id);
        Task<AddressResponse> SetDefaultAsync(int id);
    }
}