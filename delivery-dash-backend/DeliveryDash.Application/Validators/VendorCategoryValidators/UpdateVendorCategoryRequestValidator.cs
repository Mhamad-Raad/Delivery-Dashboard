using FluentValidation;
using DeliveryDash.Application.Requests.VendorCategoryRequests;

namespace DeliveryDash.Application.Validators.VendorCategoryValidators
{
    public class UpdateVendorCategoryRequestValidator : AbstractValidator<UpdateVendorCategoryRequest>
    {
        public UpdateVendorCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Vendor category name cannot exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => x.Description != null);
        }
    }
}
