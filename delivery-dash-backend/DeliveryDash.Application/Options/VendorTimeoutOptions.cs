namespace DeliveryDash.Application.Options
{
    public class VendorTimeoutOptions
    {
        public const string SectionName = "VendorTimeout";
        
        public int TimeoutMinutes { get; set; } = 1;
        public int CheckIntervalSeconds { get; set; } = 30;
    }
}