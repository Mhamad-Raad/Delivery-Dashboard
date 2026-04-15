namespace MalDash.Domain.Exceptions.BuildingExceptions
{
    public class ApartmentNotFoundException(string message) 
        : Exception(message);
}