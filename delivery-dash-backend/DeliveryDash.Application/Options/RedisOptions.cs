namespace DeliveryDash.Application.Options
{
    public class RedisOptions
    {
        public const string SectionName = "Redis";
        
        public string ConnectionString { get; set; } = string.Empty;
        public int DefaultDatabaseId { get; set; } = 0;
        public bool AbortOnConnectFail { get; set; } = false;
        public int ConnectTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;
        public string InstanceName { get; set; } = "DeliveryDash:";
    }
}