using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Domain.Entities
{
    public class VendorStaff
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int VendorId { get; set; }
        public DateTime AssignedDate { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Vendor Vendor { get; set; } = null!;
    }
}