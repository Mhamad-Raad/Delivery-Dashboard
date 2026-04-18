using FluentValidation;
using DeliveryDash.Application.Requests.VendorStaffRequests;

namespace DeliveryDash.Application.Validators.VendorStaffValidators
{
    public class CreateVendorStaffRequestValidator : AbstractValidator<CreateVendorStaffRequest>
    {
        public CreateVendorStaffRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(25).WithMessage("First name must not exceed 25 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(25).WithMessage("Last name must not exceed 25 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");
        }
    }
}