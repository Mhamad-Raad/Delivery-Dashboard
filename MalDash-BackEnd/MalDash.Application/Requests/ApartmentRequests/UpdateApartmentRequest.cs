using MalDash.Application.Requests.ApartmentRequests.FloorPlan;

namespace MalDash.Application.Requests.ApartmentRequests
{
    public record UpdateApartmentRequest
    {
        public required string ApartmentName { get; init; }
        public Guid? UserId { get; init; }
        public ApartmentLayoutRequest? Layout { get; init; }
    }
}