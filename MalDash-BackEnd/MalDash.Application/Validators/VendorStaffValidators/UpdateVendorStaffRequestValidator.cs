using FluentValidation;
using MalDash.Application.Requests.VendorStaffRequests;

namespace MalDash.Application.Validators.VendorStaffValidators
{
    public class UpdateVendorStaffRequestValidator : AbstractValidator<UpdateVendorStaffRequest>
    {
        public UpdateVendorStaffRequestValidator()
        {
            When(x => x.FirstName != null, () =>
            {
                RuleFor(x => x.FirstName)
                    .NotEmpty().WithMessage("First name cannot be empty")
                    .MaximumLength(25).WithMessage("First name must not exceed 25 characters");
            });

            When(x => x.LastName != null, () =>
            {
                RuleFor(x => x.LastName)
                    .NotEmpty().WithMessage("Last name cannot be empty")
                    .MaximumLength(25).WithMessage("Last name must not exceed 25 characters");
            });

            When(x => x.Email != null, () =>
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email cannot be empty")
                    .EmailAddress().WithMessage("Invalid email format");
            });

            When(x => x.PhoneNumber != null, () =>
            {
                RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number cannot be empty")
                    .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");
            });
        }
    }
}