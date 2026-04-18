namespace DeliveryDash.Domain.Exceptions.AddressExceptions
{
    public class ApartmentAlreadyOccupiedException(string buildingName, int floor, string apartmentName) 
        : Exception($"Apartment {apartmentName} on floor {floor} in building {buildingName} is already occupied.");
}