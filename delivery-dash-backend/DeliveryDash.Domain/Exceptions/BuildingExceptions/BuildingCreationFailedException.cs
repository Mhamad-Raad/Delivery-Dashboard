namespace DeliveryDash.Domain.Exceptions.BuildingExceptions
{
    public class BuildingCreationFailedException(string message, Exception innerException)
        : Exception(message, innerException);
}
