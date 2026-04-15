namespace MalDash.Domain.Exceptions.UserExceptions
{
    public class UserDeletionFailedException(IEnumerable<string> errorDescriptions)
        : Exception($"User deletion failed with following errors: {string.Join(Environment.NewLine, errorDescriptions)}");
}