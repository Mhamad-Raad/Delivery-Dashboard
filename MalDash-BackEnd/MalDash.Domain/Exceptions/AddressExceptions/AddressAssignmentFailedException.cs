namespace MalDash.Domain.Exceptions.AddressExceptions
{
    public class AddressAssignmentFailedException(string message, Exception innerException)
        : Exception(message, innerException);
}