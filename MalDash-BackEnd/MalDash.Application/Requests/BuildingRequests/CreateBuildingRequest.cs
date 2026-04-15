using MalDash.Application.Requests.FloorRequests;

namespace MalDash.Application.Requests.BuildingRequests
{
    public record CreateBuildingRequest
    {
        public required string Name { get; init; }
        public required List<FloorRequest> Floors { get; init; }
    }
}
