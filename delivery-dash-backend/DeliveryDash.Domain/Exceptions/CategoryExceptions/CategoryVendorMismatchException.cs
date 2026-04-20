namespace DeliveryDash.Domain.Exceptions.CategoryExceptions
{
    public class CategoryVendorMismatchException : Exception
    {
        public CategoryVendorMismatchException()
            : base("The category does not belong to the specified vendor.")
        {
        }
    }
}
