using MalDash.Domain.Enums;

namespace MalDash.Application.Responses.OrderResponses
{
    public class OrderAssignmentResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Guid DriverId { get; set; }
        public DateTime OfferedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public OrderAssignmentStatus Status { get; set; }
    }
}