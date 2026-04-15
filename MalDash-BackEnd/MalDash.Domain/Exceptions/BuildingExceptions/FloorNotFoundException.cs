namespace MalDash.Domain.Exceptions.BuildingExceptions
{
    public class FloorNotFoundException(string message) 
        : Exception(message);
}