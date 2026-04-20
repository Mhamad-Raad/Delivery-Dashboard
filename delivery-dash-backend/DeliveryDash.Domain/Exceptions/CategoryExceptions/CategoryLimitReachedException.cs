namespace DeliveryDash.Domain.Exceptions.CategoryExceptions
{
    public class CategoryLimitReachedException : Exception
    {
        public CategoryLimitReachedException(int limit)
            : base($"Category limit of {limit} per vendor has been reached.")
        {
        }
    }
}
