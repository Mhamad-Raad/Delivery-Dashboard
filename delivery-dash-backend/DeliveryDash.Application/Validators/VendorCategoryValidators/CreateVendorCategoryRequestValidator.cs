using FluentValidation;
using DeliveryDash.Application.Requests.VendorCategoryRequests;

namespace DeliveryDash.Application.Validators.VendorCategoryValidators
{
    public class CreateVendorCategoryRequestValidator : AbstractValidator<CreateVendorCategoryRequest>
    {
        public CreateVendorCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Vendor category name is required")
                .MaximumLength(100).WithMessage("Vendor category name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
