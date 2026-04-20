namespace DeliveryDash.Domain.Exceptions.VendorCategoryExceptions
{
    public class VendorCategoryInUseException : Exception
    {
        public int VendorsUsingCount { get; }

        public VendorCategoryInUseException(int id, int vendorsUsingCount)
            : base($"Cannot delete vendor category with ID {id} because {vendorsUsingCount} vendor(s) reference it. Reassign them first or set IsActive = false.")
        {
            VendorsUsingCount = vendorsUsingCount;
        }
    }
}
