using FluentValidation;
using DeliveryDash.Application.Requests.VendorRequests;

namespace DeliveryDash.Application.Validators.VendorValidators
{
    public class CreateVendorRequestValidator : AbstractValidator<CreateVendorRequest>
    {
        public CreateVendorRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Vendor name is required")
                .MaximumLength(100).WithMessage("Vendor name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.ProfileImageUrl)
                .MaximumLength(2048).WithMessage("Image URL cannot exceed 2048 characters")
                .Must(BeAValidUrl).WithMessage("Image URL must be a valid URL")
                .When(x => !string.IsNullOrWhiteSpace(x.ProfileImageUrl));

            RuleFor(x => x.OpeningTime)
                .NotEmpty().WithMessage("Opening time is required");

            RuleFor(x => x.CloseTime)
                .NotEmpty().WithMessage("Closing time is required")
                .GreaterThan(x => x.OpeningTime).WithMessage("Closing time must be after opening time");

            RuleFor(x => x.VendorCategoryId)
                .GreaterThan(0).WithMessage("Vendor category is required");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");
        }

        private static bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
