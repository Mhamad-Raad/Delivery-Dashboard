using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Requests.AddressRequests
{
    public class CreateAddressRequest
    {
        public AddressType Type { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;

        public string? BuildingName { get; set; }
        public string? Floor { get; set; }
        public string? ApartmentNumber { get; set; }
        public string? HouseName { get; set; }
        public string? HouseNumber { get; set; }
        public string? CompanyName { get; set; }

        public string? AdditionalDirections { get; set; }
        public string? Label { get; set; }

        public bool IsDefault { get; set; }
    }
}
