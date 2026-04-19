namespace DeliveryDash.Domain.Exceptions.AddressExceptions
{
    public class AdminsCannotOwnAddressException : Exception
    {
        public AdminsCannotOwnAddressException()
            : base("Admin users cannot create or own addresses.")
        {
        }
    }
}
