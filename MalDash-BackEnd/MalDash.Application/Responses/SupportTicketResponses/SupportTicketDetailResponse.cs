using MalDash.Domain.Enums;

namespace MalDash.Application.Responses.SupportTicketResponses
{
    public class SupportTicketDetailResponse
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? AdminNotes { get; set; }
        public List<string> ImageUrls { get; set; } = [];
    }
}