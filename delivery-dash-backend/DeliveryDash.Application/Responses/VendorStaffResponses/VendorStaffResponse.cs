using DeliveryDash.Application.Responses.VendorResponses;

namespace DeliveryDash.Application.Responses.VendorStaffResponses
{
    public class VendorStaffResponse
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public bool IsActive { get; set; }
        public VendorDetailResponse? VendorProfile { get; internal set; }
    }
}