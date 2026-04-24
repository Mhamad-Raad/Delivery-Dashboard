using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Domain.Entities
{
    public class DeviceToken
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DevicePlatform Platform { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
