using FluentValidation;
using DeliveryDash.Application.Requests.BuildingRequests;
using DeliveryDash.Application.Validators.FloorValidator;

namespace DeliveryDash.Application.Validators.BuildingValidator
{
    public class CreateBuildingRequestValidator : AbstractValidator<CreateBuildingRequest>
    {
        public CreateBuildingRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmptyWithException("Building name")
                .MaximumLength(100).WithMessage("Building name must not exceed 100 characters");

            RuleFor(x => x.Floors)
                .NotEmpty().WithMessage("At least one floor is required")
                .Must(floors => floors != null && floors.Count > 0)
                .WithMessage("Building must have at least one floor");

            RuleForEach(x => x.Floors).SetValidator(new FloorRequestValidator());
        }
    }
}