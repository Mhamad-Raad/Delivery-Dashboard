using MalDash.Domain.Enums;

namespace MalDash.Application.Requests.VendorRequests
{
    public class UpdateVendorRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProfileImageUrl { get; set; }
        public TimeSpan? OpeningTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public VendorType? Type { get; set; }
    }
}