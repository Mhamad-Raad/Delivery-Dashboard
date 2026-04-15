namespace MalDash.Domain.Exceptions.AddressExceptions
{
    public class AddressCreationFailedException(string message, Exception innerException)
        : Exception(message, innerException);
}