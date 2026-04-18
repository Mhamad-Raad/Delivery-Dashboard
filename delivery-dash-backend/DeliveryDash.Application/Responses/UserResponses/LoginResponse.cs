using DeliveryDash.Application.Responses.VendorResponses;

namespace DeliveryDash.Application.Responses.UserResponses
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
        public UserInfo User { get; set; } = null!;
        public VendorDetailResponse? VendorProfile { get; set; }
    }
}
