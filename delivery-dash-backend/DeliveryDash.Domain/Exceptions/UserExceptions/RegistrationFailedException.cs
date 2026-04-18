namespace DeliveryDash.Domain.Exceptions.UserExceptions
{
    public class RegistrationFailedException(IEnumerable<string> errorDescriptions)
            : Exception($"Registration failed with following errors: {string.Join(Environment.NewLine, errorDescriptions)}");
}
