namespace DeliveryDash.Domain.Exceptions.AddressExceptions
{
    public class AddressUnassignmentFailedException(string message, Exception innerException) 
        : Exception(message, innerException);
}