namespace MalDash.Domain.Exceptions.AddressExceptions
{
    public class AddressUpdateFailedException(string message, Exception innerException) 
        : Exception(message, innerException);
}