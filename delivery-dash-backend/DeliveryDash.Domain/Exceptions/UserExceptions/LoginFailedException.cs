namespace DeliveryDash.Domain.Exceptions.UserExceptions
{
    public class LoginFailedException(string email) 
        : Exception($"Invalid email: {email} or password.");
}
