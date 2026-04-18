namespace DeliveryDash.Domain.Exceptions.VendorExceptions
{
    public class VendorStaffNotFoundException : Exception
    {
        public VendorStaffNotFoundException(int id)
            : base($"Vendor staff with ID {id} was not found.")
        {
        }
    }
}