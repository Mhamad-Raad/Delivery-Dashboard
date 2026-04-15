using MalDash.Domain.Enums;

namespace MalDash.Application.Responses.SupportTicketResponses
{
    public class SupportTicketResponse
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}