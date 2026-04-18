namespace DeliveryDash.Domain.Exceptions.VendorExceptions
{
    public class UserNotVendorRoleException : Exception
    {
        public UserNotVendorRoleException(Guid userId) 
            : base($"User with ID '{userId}' does not have the Vendor role and cannot be assigned to a vendor.")
        {
        }
    }
}