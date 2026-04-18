namespace DeliveryDash.Domain.Exceptions.UserExceptions
{
    public class UserUpdateFailedException(IEnumerable<string> errorDescriptions)
        : Exception($"User update failed with following errors: {string.Join(Environment.NewLine, errorDescriptions)}");
}