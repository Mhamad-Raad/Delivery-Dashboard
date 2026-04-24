using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Requests.NotificationRequests
{
    public class RegisterDeviceTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public DevicePlatform Platform { get; set; } = DevicePlatform.Android;
    }
}
