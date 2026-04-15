using FluentValidation;
using MalDash.Application.Requests.VendorRequests;

namespace MalDash.Application.Validators.VendorValidators
{
    public class UpdateVendorRequestValidator : AbstractValidator<UpdateVendorRequest>
    {
        public UpdateVendorRequestValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Vendor name cannot exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => x.Description != null);

            RuleFor(x => x.ProfileImageUrl)
                .MaximumLength(2048).WithMessage("Image URL cannot exceed 2048 characters")
                .Must(BeAValidUrl).WithMessage("Image URL must be a valid URL")
                .When(x => !string.IsNullOrWhiteSpace(x.ProfileImageUrl));

            RuleFor(x => x.CloseTime)
                .GreaterThan(x => x.OpeningTime!.Value)
                .WithMessage("Closing time must be after opening time")
                .When(x => x.OpeningTime.HasValue && x.CloseTime.HasValue);

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid vendor type")
                .When(x => x.Type.HasValue);
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