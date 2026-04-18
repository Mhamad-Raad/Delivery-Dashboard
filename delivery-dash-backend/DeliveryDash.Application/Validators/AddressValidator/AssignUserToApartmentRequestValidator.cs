using FluentValidation;
using DeliveryDash.Application.Requests.AddressRequests;

namespace DeliveryDash.Application.Validators.AddressValidator
{
    public class AssignUserToApartmentRequestValidator : AbstractValidator<AssignUserToApartmentRequest>
    {
        public AssignUserToApartmentRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .Must(id => id != Guid.Empty).WithMessage("User ID cannot be empty");

            RuleFor(x => x.BuildingId)
                .GreaterThan(0).WithMessage("Building ID must be greater than 0");

            RuleFor(x => x.FloorNumber)
                .GreaterThan(0).WithMessage("Floor number must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Floor number cannot exceed 100");

            RuleFor(x => x.ApartmentName)
                .NotEmpty().WithMessage("Apartment name is required")
                .MaximumLength(50).WithMessage("Apartment name cannot exceed 50 characters");
        }
    }
}