namespace DeliveryDash.Domain.Exceptions.VendorCategoryExceptions
{
    public class DuplicateVendorCategoryNameException : Exception
    {
        public DuplicateVendorCategoryNameException(string name)
            : base($"A vendor category with the name '{name}' already exists.")
        {
        }
    }
}
