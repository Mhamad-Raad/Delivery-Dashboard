using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Domain.Entities
{
    public class OrderAssignment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Guid DriverId { get; set; }
        public int? ShiftId { get; set; }
        public DateTime OfferedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public OrderAssignmentStatus Status { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
        public User Driver { get; set; } = null!;
        public DriverShift? Shift { get; set; }
    }
}