namespace MalDash.Domain.Exceptions.UserExceptions
{
    public class UserAlreadyVendorStaffException : Exception
    {
        public UserAlreadyVendorStaffException(Guid userId)
            : base($"User with ID {userId} is already assigned as vendor staff.")
        {
        }
    }
}