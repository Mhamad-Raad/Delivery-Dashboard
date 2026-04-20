using FluentValidation;
using DeliveryDash.Application.Requests.CategoryRequests;

namespace DeliveryDash.Application.Validators.CategoryValidators
{
    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmptyWithException("Category name")
                .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order must be zero or greater")
                .When(x => x.SortOrder.HasValue);
        }
    }
}
