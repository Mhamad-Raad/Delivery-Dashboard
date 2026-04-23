using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Responses.OrderResponses
{
    public class OrderDetailResponse
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public DeliveryAddressInfo? DeliveryAddress { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? PreparingAt { get; set; }
        public DateTime? OutForDeliveryAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
    }

    public class OrderItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class DeliveryAddressInfo
    {
        public int Id { get; set; }
        public AddressType Type { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Street { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? BuildingName { get; set; }
        public string? Floor { get; set; }
        public string? ApartmentNumber { get; set; }
        public string? HouseName { get; set; }
        public string? HouseNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? AdditionalDirections { get; set; }
        public string? Label { get; set; }
    }
}
