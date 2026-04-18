using FluentValidation;
using DeliveryDash.Application.Requests.ProductRequests;

namespace DeliveryDash.Application.Validators.ProductValidators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be greater than 0")
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.Name)
                .NotEmptyWithException("Product name")
                .MaximumLength(30).WithMessage("Product name cannot exceed 30 characters");

            RuleFor(x => x.Description)
                .NotEmptyWithException("Product description")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .Must(BeValidDecimal).WithMessage("Price must have at most 2 decimal places and 10 total digits");

            RuleFor(x => x.DiscountPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Discount price must be greater than 0")
                .LessThan(x => x.Price).WithMessage("Discount price must be less than the regular price")
                .Must(BeValidDecimal).WithMessage("Discount price must have at most 2 decimal places and 10 total digits")
                .When(x => x.DiscountPrice.HasValue);
        }

        private static bool BeValidDecimal(decimal value)
        {
            // Check decimal(10,2): max 10 total digits, 2 after decimal
            var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
            if (decimalPlaces > 2)
                return false;

            // Check total digits (precision)
            var totalDigits = value.ToString("0.##").Replace(".", "").Replace("-", "").Length;
            return totalDigits <= 10;
        }

        private static bool BeValidDecimal(decimal? value)
        {
            return !value.HasValue || BeValidDecimal(value.Value);
        }
    }
}