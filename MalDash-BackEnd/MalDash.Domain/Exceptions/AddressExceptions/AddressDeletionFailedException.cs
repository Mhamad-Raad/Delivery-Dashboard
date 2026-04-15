namespace MalDash.Domain.Exceptions.AddressExceptions
{
    public class AddressDeletionFailedException(string message, Exception innerException)
        : Exception(message, innerException);
}