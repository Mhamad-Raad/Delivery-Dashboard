using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Domain.Entities
{
    public class SupportTicket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; } = TicketStatus.Open;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public string? AdminNotes { get; set; }

        // Navigation
        public User? User { get; set; }
        public List<EntityImage> Images { get; set; } = [];
    }
}