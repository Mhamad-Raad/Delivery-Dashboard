namespace DeliveryDash.Application.Responses.NotificationResponses
{
    public class BroadcastSummaryResponse
    {
        public int Key { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Recipients { get; set; }
    }
}
