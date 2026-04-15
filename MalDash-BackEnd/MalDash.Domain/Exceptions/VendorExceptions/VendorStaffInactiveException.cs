namespace MalDash.Domain.Exceptions.VendorExceptions
{
    public class VendorStaffInactiveException : Exception
    {
        public VendorStaffInactiveException()
            : base("Your staff account has been deactivated. Please contact your vendor manager.")
        {
        }

        public VendorStaffInactiveException(string email)
            : base($"The staff account for {email} has been deactivated. Please contact your vendor manager.")
        {
        }
    }
}