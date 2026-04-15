using MalDash.Domain.Enums;

namespace MalDash.Application.Requests.VendorRequests
{
    public class CreateVendorRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public VendorType Type { get; set; }
        public Guid UserId { get; set; }
    }
}