namespace DeliveryDash.Application.Responses.ApartmentResponses
{
    public class UserApartmentResponse
    {
        public int AddressId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string BuildingName { get; set; }
        public int FloorNumber { get; set; }
        public string ApartmentName { get; set; }
    }
}