namespace DeliveryDash.Application.Responses.DashboardResponses
{
    public class AdminAnalyticsResponse
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public string Granularity { get; set; } = "day";

        public FinancialSection Financial { get; set; } = new();
        public OrdersSection Orders { get; set; } = new();
        public VendorsSection Vendors { get; set; } = new();
        public DriversSection Drivers { get; set; } = new();
        public CustomersSection Customers { get; set; } = new();
        public SupportSection Support { get; set; } = new();
    }

    public class FinancialSection
    {
        public decimal TotalRevenue { get; set; }
        public decimal Gmv { get; set; }
        public decimal AvgOrderValue { get; set; }
        public List<TimePoint> RevenueSeries { get; set; } = [];
        public List<NamedAmount> RevenueByCategory { get; set; } = [];
    }

    public class OrdersSection
    {
        public int Total { get; set; }
        public decimal CancellationRate { get; set; }
        public List<TimePoint> OrdersSeries { get; set; } = [];
        public List<StatusCount> StatusBreakdown { get; set; } = [];
    }

    public class VendorsSection
    {
        public int NewSignups { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public List<VendorRanking> TopByRevenue { get; set; } = [];
        public List<VendorRanking> TopByOrders { get; set; } = [];
    }

    public class DriversSection
    {
        public int ActiveCount { get; set; }
        public double AvgDeliveryMinutes { get; set; }
        public List<DriverRanking> TopByDeliveries { get; set; } = [];
    }

    public class CustomersSection
    {
        public int NewSignups { get; set; }
        public int Returning { get; set; }
        public int OneTime { get; set; }
        public List<TimePoint> SignupSeries { get; set; } = [];
        public List<CustomerRanking> TopSpenders { get; set; } = [];
    }

    public class SupportSection
    {
        public int Opened { get; set; }
        public int Resolved { get; set; }
        public int OpenBacklog { get; set; }
        public double AvgResolutionHours { get; set; }
    }

    public class TimePoint
    {
        public DateTime Bucket { get; set; }
        public decimal Value { get; set; }
    }

    public class StatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class NamedAmount
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class VendorRanking
    {
        public int VendorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DriverRanking
    {
        public Guid DriverId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Deliveries { get; set; }
        public double AvgDeliveryMinutes { get; set; }
    }

    public class CustomerRanking
    {
        public Guid CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
