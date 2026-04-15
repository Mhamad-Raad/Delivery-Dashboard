using MalDash.Domain.Enums;

namespace MalDash.Application.Requests.VendorStaffRequests
{
    public class UpdateVendorStaffRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
    }
}