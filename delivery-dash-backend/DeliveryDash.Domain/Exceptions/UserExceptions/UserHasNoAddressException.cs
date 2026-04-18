namespace DeliveryDash.Domain.Exceptions.UserExceptions
{
    public class UserHasNoAddressException : Exception
    {
        public UserHasNoAddressException() 
            : base("User does not have an assigned address.")
        {
        }
    }
}