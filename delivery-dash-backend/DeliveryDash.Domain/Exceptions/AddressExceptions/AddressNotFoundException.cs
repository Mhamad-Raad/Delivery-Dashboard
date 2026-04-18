namespace DeliveryDash.Domain.Exceptions.AddressExceptions
{
    public class AddressNotFoundException : Exception
    {
        public AddressNotFoundException() 
            : base("Address not found.")
        {
        }

        public AddressNotFoundException(int id) 
            : base($"Address with ID {id} not found.")
        {
        }
    }
}