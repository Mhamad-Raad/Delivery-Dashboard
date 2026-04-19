using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Responses.UserResponses
{
    public class UserResponse
    {
        public Guid _id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Role Role { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
