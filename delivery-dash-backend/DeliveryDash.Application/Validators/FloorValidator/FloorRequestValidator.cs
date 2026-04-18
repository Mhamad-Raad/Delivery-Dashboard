using FluentValidation;
using DeliveryDash.Application.Requests.FloorRequests;

namespace DeliveryDash.Application.Validators.FloorValidator
{
    public class FloorRequestValidator : AbstractValidator<FloorRequest>
    {
        public FloorRequestValidator()
        {
            RuleFor(x => x.FloorNumber)
                .NotNull().WithMessage("Floor number is required")
                .GreaterThan(0).WithMessage("Floor number must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Floor number cannot exceed 100");

            RuleFor(x => x.NumberOfApartments)
                .NotNull().WithMessage("Number of apartments is required")
                .GreaterThan(0).WithMessage("Number of apartments must be greater than 0")
                .LessThanOrEqualTo(50).WithMessage("Number of apartments cannot exceed 50 per floor");
        }
    }
}