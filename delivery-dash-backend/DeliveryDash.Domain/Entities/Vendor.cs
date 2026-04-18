using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Domain.Entities
{
    public class Vendor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public VendorType Type { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
