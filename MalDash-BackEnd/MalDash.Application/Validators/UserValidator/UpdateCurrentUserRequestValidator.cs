using FluentValidation;
using MalDash.Application.Requests.UserRequest;

namespace MalDash.Application.Validators.UserValidator
{
    public class UpdateCurrentUserRequestValidator : AbstractValidator<UpdateCurrentUserRequest>
    {
        public UpdateCurrentUserRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(25).WithMessage("First name must not exceed 25 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(25).WithMessage("Last name must not exceed 25 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.LastName));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\d+$").WithMessage("Phone number must contain only numeric digits.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }
}