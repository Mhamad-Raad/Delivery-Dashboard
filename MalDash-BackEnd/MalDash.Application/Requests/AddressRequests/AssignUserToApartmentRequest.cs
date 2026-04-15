namespace MalDash.Application.Requests.AddressRequests
{
    public record AssignUserToApartmentRequest
    {
        public required Guid UserId { get; init; }
        public required int BuildingId { get; init; }
        public required int FloorNumber { get; init; }
        public required string ApartmentName { get; init; }
    }
}