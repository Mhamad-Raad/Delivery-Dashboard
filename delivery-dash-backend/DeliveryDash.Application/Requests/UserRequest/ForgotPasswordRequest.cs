namespace DeliveryDash.Application.Requests.UserRequest
{
    public record ForgotPasswordRequest
    {
        public required string Email { get; init; }
    }
}