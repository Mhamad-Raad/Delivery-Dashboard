using FluentValidation;
using DeliveryDash.Application.Requests.AddressRequests;
using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Validators.AddressValidator
{
    public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
    {
        public CreateAddressRequestValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");

            RuleFor(x => x.PhoneNumber)
                .NotEmptyWithException("Phone number")
                .MaximumLength(30).WithMessage("Phone number cannot exceed 30 characters.");

            RuleFor(x => x.Street)
                .NotEmptyWithException("Street")
                .MaximumLength(255).WithMessage("Street cannot exceed 255 characters.");

            RuleFor(x => x.Label)
                .MaximumLength(50).WithMessage("Label cannot exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Label));

            RuleFor(x => x.AdditionalDirections)
                .MaximumLength(500).WithMessage("Additional directions cannot exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.AdditionalDirections));

            // Apartment-specific
            When(x => x.Type == AddressType.Apartment, () =>
            {
                RuleFor(x => x.BuildingName).NotEmpty().WithMessage("Building name is required.");
                RuleFor(x => x.Floor).NotEmpty().WithMessage("Floor is required.");
                RuleFor(x => x.ApartmentNumber).NotEmpty().WithMessage("Apartment number is required.");
            });

            // House-specific
            When(x => x.Type == AddressType.House, () =>
            {
                RuleFor(x => x)
                    .Must(x => !string.IsNullOrWhiteSpace(x.HouseName) || !string.IsNullOrWhiteSpace(x.HouseNumber))
                    .WithMessage("Either HouseName or HouseNumber is required for a house address.")
                    .WithErrorCode("HouseIdentifierRequired");
            });

            // Office-specific
            When(x => x.Type == AddressType.Office, () =>
            {
                RuleFor(x => x.BuildingName).NotEmpty().WithMessage("Building name is required.");
                RuleFor(x => x.CompanyName).NotEmpty().WithMessage("Company name is required.");
                RuleFor(x => x.Floor).NotEmpty().WithMessage("Floor is required.");
            });
        }
    }
}
