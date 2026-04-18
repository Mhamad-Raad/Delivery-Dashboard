namespace DeliveryDash.Domain.Exceptions.VendorExceptions
{
    public class DuplicateVendorNameException : Exception
    {
        public DuplicateVendorNameException(string vendorName) 
            : base($"A vendor with the name '{vendorName}' already exists.")
        {
        }
    }
}