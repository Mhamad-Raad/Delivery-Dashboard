namespace MalDash.Application.Responses.DashboardResponses
{
    public class VendorDashboardResponse
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TodayOrders { get; set; }
        public List<DailyOrderStatResponse> DailyStats { get; set; } = [];
    }

    public class DailyOrderStatResponse
    {
        public string Day { get; set; } = string.Empty;
        public int Orders { get; set; }
        public decimal Profit { get; set; }
    }
}