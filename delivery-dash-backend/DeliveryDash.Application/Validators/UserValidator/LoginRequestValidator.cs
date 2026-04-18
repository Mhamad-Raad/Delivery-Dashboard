using FluentValidation;
using DeliveryDash.Application.Requests.UserRequest;

namespace DeliveryDash.Application.Validators.UserValidator
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmptyWithException("Password");

            RuleFor(x => x.ApplicationContext)
                .Must(context => !string.IsNullOrWhiteSpace(context));
        }
    }
}
