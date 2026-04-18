namespace DeliveryDash.Application.Options
{
    public class DashboardCacheOptions
    {
        public const string SectionName = "DashboardCache";

        public int AdminDashboardCacheDurationMinutes { get; set; } = 30;

        public int VendorDashboardCacheDurationMinutes { get; set; } = 30;
    }
}