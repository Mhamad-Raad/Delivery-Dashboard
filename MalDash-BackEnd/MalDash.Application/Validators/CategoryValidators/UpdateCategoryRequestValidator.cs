using FluentValidation;
using MalDash.Application.Requests.CategoryRequests;

namespace MalDash.Application.Validators.CategoryValidators
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

            RuleFor(x => x.ParentCategoryId)
                .GreaterThan(0).WithMessage("Parent category ID must be greater than 0")
                .When(x => x.ParentCategoryId.HasValue);
        }
    }
}