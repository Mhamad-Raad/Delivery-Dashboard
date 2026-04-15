using MalDash.Domain.Enums;

namespace MalDash.Domain.Exceptions.OrderExceptions
{
    public class InvalidOrderStatusTransitionException : Exception
    {
        public InvalidOrderStatusTransitionException(OrderStatus from, OrderStatus to)
            : base($"Cannot transition order status from {from} to {to}")
        {
        }
    }
}