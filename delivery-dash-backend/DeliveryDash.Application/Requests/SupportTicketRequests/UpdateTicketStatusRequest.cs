using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Requests.SupportTicketRequests
{
    public class UpdateTicketStatusRequest
    {
        public required TicketStatus Status { get; set; }
        public string? AdminNotes { get; set; }
    }
}