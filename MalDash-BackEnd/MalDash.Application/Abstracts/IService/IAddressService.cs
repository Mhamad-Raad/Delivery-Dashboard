using MalDash.Application.Requests.AddressRequests;
using MalDash.Application.Responses.ApartmentResponses;

namespace MalDash.Application.Abstracts.IService
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