using FluentValidation;
using MalDash.Application.Requests.BuildingRequests;
using MalDash.Application.Validators.FloorValidator;

namespace MalDash.Application.Validators.BuildingValidator
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