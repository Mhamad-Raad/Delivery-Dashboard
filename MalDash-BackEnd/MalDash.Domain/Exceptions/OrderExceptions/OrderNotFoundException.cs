namespace MalDash.Domain.Exceptions.OrderExceptions
{
    public class OrderNotFoundException : Exception
    {
        public OrderNotFoundException()
            : base("Order not found")
        {
        }

        public OrderNotFoundException(int orderId)
            : base($"Order with ID {orderId} was not found")
        {
        }

        public OrderNotFoundException(string orderNumber)
            : base($"Order with number {orderNumber} was not found")
        {
        }
    }
}