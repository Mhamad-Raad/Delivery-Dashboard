using MalDash.Domain.Enums;

namespace MalDash.Application.Requests.UserRequest
{
    public record UpdateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Role Role { get; init; }
    }
}