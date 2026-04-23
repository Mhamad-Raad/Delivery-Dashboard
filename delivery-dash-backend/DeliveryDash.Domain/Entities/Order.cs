using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public int VendorId { get; set; }
        public int? AddressId { get; set; }
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

        // Navigation properties
        public User User { get; set; } = null!;
        public Vendor Vendor { get; set; } = null!;
        public Address? DeliveryAddress { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}