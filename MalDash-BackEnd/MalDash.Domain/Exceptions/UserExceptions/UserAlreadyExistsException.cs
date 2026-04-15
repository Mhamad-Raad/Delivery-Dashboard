namespace MalDash.Domain.Exceptions.UserExceptions
{
    public class UserAlreadyExistsException(string email) 
        : Exception($"User with email: {email} already exists");
}
