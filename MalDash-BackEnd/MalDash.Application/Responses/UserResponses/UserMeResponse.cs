using MalDash.Application.Responses.VendorResponses;
using MalDash.Application.Responses.VendorStaffResponses;

namespace MalDash.Application.Responses.UserResponses
{
    public class UserMeResponse
    {
        public UserInfo User { get; set; } = null!;
        public VendorDetailResponse? VendorProfile { get; set; }
        public VendorStaffResponse? StaffProfile { get; set; }
    }
}