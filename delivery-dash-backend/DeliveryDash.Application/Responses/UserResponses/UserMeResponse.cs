using DeliveryDash.Application.Responses.VendorResponses;
using DeliveryDash.Application.Responses.VendorStaffResponses;

namespace DeliveryDash.Application.Responses.UserResponses
{
    public class UserMeResponse
    {
        public UserInfo User { get; set; } = null!;
        public VendorDetailResponse? VendorProfile { get; set; }
        public VendorStaffResponse? StaffProfile { get; set; }
    }
}