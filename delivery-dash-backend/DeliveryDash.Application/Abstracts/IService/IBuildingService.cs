using DeliveryDash.Application.Requests.ApartmentRequests;
using DeliveryDash.Application.Requests.BuildingRequests;
using DeliveryDash.Application.Responses.ApartmentResponses;
using DeliveryDash.Application.Responses.BuildingResponses;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.FloorResponses;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IBuildingService
    {
        Task<BuildingDetailResponse> GetBuildingByIdAsync(int id);
        Task<PagedResponse<BuildingResponse>> GetAllBuildingsAsync(int page = 1, int limit = 10, string? searchName = null);
        Task<BuildingResponse> CreateBuildingAsync(CreateBuildingRequest request);
        Task UpdateBuildingAsync(int id, UpdateBuildingRequest request);
        Task DeleteBuildingAsync(int id);
        Task<FloorDetailResponse> AddFloorToBuildingAsync(int buildingId);
        Task DeleteFloorAsync(int floorId);
        Task<ApartmentDetailResponse> AddApartmentToFloorAsync(int floorId, AddApartmentRequest request);
        Task UpdateApartmentAsync(int apartmentId, UpdateApartmentRequest request);
        Task DeleteApartmentAsync(int apartmentId);
    }
}