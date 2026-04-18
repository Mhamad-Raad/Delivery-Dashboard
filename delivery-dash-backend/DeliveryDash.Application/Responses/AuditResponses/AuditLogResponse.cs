namespace DeliveryDash.Application.Responses.AuditResponses
{
    public class AuditLogResponse
    {
        public long Id { get; set; }
        public Guid? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, object?>? OldValues { get; set; }
        public Dictionary<string, object?>? NewValues { get; set; }
        public List<string>? AffectedColumns { get; set; }
        public DateTime Timestamp { get; set; }
    }
}