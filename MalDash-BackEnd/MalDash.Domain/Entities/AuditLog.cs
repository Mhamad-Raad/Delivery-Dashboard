namespace MalDash.Domain.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }
        public Guid? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Created, Updated, Deleted
        public string? OldValues { get; set; } // JSON
        public string? NewValues { get; set; } // JSON
        public string? AffectedColumns { get; set; } // JSON array
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? UserAgent { get; set; }

        // Navigation
        public User? User { get; set; }
    }
}