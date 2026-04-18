using DeliveryDash.Application.Requests.FloorRequests;

namespace DeliveryDash.Application.Requests.BuildingRequests
{
    public record CreateBuildingRequest
    {
        public required string Name { get; init; }
        public required List<FloorRequest> Floors { get; init; }
    }
}
