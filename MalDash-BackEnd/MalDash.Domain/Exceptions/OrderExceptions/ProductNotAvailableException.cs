namespace MalDash.Domain.Exceptions.OrderExceptions
{
    public class ProductNotAvailableException : Exception
    {
        public ProductNotAvailableException(int productId)
            : base($"Product with ID {productId} is not available for order")
        {
        }

        public ProductNotAvailableException(string productName)
            : base($"Product '{productName}' is not available for order")
        {
        }
    }
}