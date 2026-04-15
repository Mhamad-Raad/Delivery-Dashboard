using MalDash.Domain.Enums;

namespace MalDash.Application.Requests.UserRequest
{
    public record RegisterRequest
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; }
        public required string PhoneNumber { get; init; }
        public required Role Role { get; init; }
    }
}
