using FluentValidation;
using DeliveryDash.Application.Requests.CategoryRequests;

namespace DeliveryDash.Application.Validators.CategoryValidators
{
    public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
    {
        public UpdateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => x.Description != null);

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order must be zero or greater")
                .When(x => x.SortOrder.HasValue);
        }
    }
}
