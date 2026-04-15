using MalDash.Domain.Enums;

namespace MalDash.Application.Requests.SupportTicketRequests
{
    public class UpdateTicketStatusRequest
    {
        public required TicketStatus Status { get; set; }
        public string? AdminNotes { get; set; }
    }
}