namespace MalDash.Application.Requests.UserRequest
{
    public record LoginRequest
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
        public required string ApplicationContext { get; init; }
    }
}
