namespace MalDash.Domain.Exceptions.VendorExceptions
{
    public class VendorNotFoundException : Exception
    {
        public VendorNotFoundException(string message) : base(message)
        {
        }
    }
}