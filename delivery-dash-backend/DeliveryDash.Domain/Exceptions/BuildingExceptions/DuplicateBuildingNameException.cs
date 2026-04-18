namespace DeliveryDash.Domain.Exceptions.BuildingExceptions
{
    public class DuplicateBuildingNameException(string buildingName)
        : Exception($"A building with the name '{buildingName}' already exists.");
}
