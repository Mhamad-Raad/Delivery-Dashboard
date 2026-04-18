namespace DeliveryDash.Application.Requests.FloorRequests
{
    public record FloorRequest
    {
        public required int FloorNumber { get; init; }
        public required int NumberOfApartments { get; init; }
    }
}
