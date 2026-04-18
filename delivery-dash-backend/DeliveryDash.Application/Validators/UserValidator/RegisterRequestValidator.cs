using FluentValidation;
using DeliveryDash.Application.Requests.UserRequest;

namespace DeliveryDash.Application.Validators.UserValidator
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmptyWithException("First name")
                .MaximumLength(25);

            RuleFor(x => x.LastName)
                .NotEmptyWithException("Last name")
                .MaximumLength(25);

            RuleFor(x => x.Email)
                .NotEmptyWithException("Email")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"\d").WithMessage("Password must contain at least one digit.")
                .Must(p => p != null && p.Distinct().Count() >= 1)
                    .WithMessage("Password must contain at least one unique character.");

            RuleFor(x => x.PhoneNumber)
                .NotEmptyWithException("Phone number")
                .Matches(@"^\d+$").WithMessage("Phone number must contain only numeric digits");

            RuleFor(x => x.Role)
                .NotNull()
                .WithMessage("Role is required.")
                .IsInEnum()
                .WithMessage("Invalid role specified.")
                .Must(role => (int)role >= 0 && (int)role <= 4)
                .WithMessage("Role must be between 0 and 4.");
        }
    }
}
