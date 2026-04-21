using FluentValidation;
using DeliveryDash.Application.Requests.UserRequest;

namespace DeliveryDash.Application.Validators.UserValidator
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
                .Matches(@"^\d{1,11}$").WithMessage("Phone number must contain only digits and be at most 11 digits.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }
}