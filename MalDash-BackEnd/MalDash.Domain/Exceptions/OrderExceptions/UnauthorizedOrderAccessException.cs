namespace MalDash.Domain.Exceptions.OrderExceptions
{
    public class UnauthorizedOrderAccessException : Exception
    {
        public UnauthorizedOrderAccessException()
            : base("You are not authorized to access this order")
        {
        }
    }
}