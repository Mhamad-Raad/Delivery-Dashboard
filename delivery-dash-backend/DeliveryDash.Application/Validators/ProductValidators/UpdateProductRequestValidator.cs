using FluentValidation;
using DeliveryDash.Application.Requests.ProductRequests;

namespace DeliveryDash.Application.Validators.ProductValidators
{
    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be greater than 0")
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.Name)
                .MaximumLength(30).WithMessage("Product name cannot exceed 30 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .Must(BeValidDecimal).WithMessage("Price must have at most 2 decimal places and 10 total digits")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.DiscountPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Discount price must be greater than 0")
                .LessThan(x => x.Price).WithMessage("Discount price must be less than the regular price")
                .Must(BeValidDecimal).WithMessage("Discount price must have at most 2 decimal places and 10 total digits")
                .When(x => x.DiscountPrice.HasValue);

            // Cross-field validation when both Price and DiscountPrice are provided
            RuleFor(x => x)
                .Must(x => !x.Price.HasValue || !x.DiscountPrice.HasValue || x.DiscountPrice < x.Price)
                .WithMessage("Discount price must be less than the regular price")
                .When(x => x.Price.HasValue && x.DiscountPrice.HasValue);
        }

        private static bool BeValidDecimal(decimal? value)
        {
            if (!value.HasValue)
                return true;

            // Check decimal(10,2): max 10 total digits, 2 after decimal
            var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value.Value)[3])[2];
            if (decimalPlaces > 2)
                return false;

            // Check total digits (precision)
            var totalDigits = value.Value.ToString("0.##").Replace(".", "").Replace("-", "").Length;
            return totalDigits <= 10;
        }
    }
}