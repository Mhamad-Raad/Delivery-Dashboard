namespace DeliveryDash.Domain.Exceptions.UserExceptions
{
    public class RefreshTokenException(string message) 
        : Exception(message);
}
