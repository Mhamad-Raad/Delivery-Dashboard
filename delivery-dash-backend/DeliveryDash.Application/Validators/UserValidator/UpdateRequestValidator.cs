using FluentValidation;
using DeliveryDash.Application.Requests.UserRequest;

public class UpdateRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required and cannot be empty.")
            .MaximumLength(25).WithMessage("First name must not exceed 25 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required and cannot be empty.")
            .MaximumLength(25).WithMessage("Last name must not exceed 25 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required and cannot be empty.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required and cannot be empty.")
            .Matches(@"^\d{1,11}$").WithMessage("Phone number must contain only digits and be at most 11 digits.");

        RuleFor(x => x.Role)
            .NotNull().WithMessage("Role is required.")
            .IsInEnum().WithMessage("Invalid role value.");
    }
}