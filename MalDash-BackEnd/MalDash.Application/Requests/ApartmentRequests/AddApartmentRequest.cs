namespace MalDash.Application.Requests.ApartmentRequests
{
    public record AddApartmentRequest
    {
        public required string ApartmentName { get; init; }
    }
}