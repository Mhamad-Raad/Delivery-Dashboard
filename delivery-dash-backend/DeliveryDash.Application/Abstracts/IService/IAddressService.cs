using DeliveryDash.Application.Requests.AddressRequests;
using DeliveryDash.Application.Responses.ApartmentResponses;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IAddressService
    {   
        Task<UserApartmentResponse> AssignUserToApartmentAsync(AssignUserToApartmentRequest request);
        Task<UserApartmentResponse?> GetUserApartmentAsync(Guid userId);
        Task<IEnumerable<UserApartmentResponse>> GetAllUserApartmentsAsync();
        Task<IEnumerable<UserApartmentResponse>> GetUsersByBuildingAsync(int buildingId);
        Task<IEnumerable<UserApartmentResponse>> GetUsersByFloorAsync(int buildingId, int floorNumber);
        Task UnassignUserFromApartmentAsync(Guid userId);
    }
}