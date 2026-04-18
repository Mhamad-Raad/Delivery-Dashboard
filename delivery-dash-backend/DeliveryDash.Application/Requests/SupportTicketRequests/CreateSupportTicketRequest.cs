using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Requests.SupportTicketRequests
{
    public class CreateSupportTicketRequest
    {
        public required string Subject { get; set; }
        public required string Description { get; set; }
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    }
}