namespace DeliveryDash.Domain.Exceptions.VendorCategoryExceptions
{
    public class VendorCategoryNotFoundException : Exception
    {
        public VendorCategoryNotFoundException(int id)
            : base($"Vendor category with ID {id} was not found.")
        {
        }
    }
}
