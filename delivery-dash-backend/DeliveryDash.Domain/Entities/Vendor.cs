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
        public int VendorCategoryId { get; set; }
        public Guid UserId { get; set; }

        public VendorCategory VendorCategory { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
