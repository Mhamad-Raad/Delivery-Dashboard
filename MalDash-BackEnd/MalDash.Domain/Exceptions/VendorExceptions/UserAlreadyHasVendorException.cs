namespace MalDash.Domain.Exceptions.VendorExceptions
{
    public class UserAlreadyHasVendorException : Exception
    {
        public UserAlreadyHasVendorException(Guid userId) 
            : base($"User with ID '{userId}' already has a vendor associated.")
        {
        }
    }
}