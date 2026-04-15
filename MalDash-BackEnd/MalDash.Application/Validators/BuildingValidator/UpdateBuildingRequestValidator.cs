using FluentValidation;
using MalDash.Application.Requests.BuildingRequests;

namespace MalDash.Application.Validators.BuildingValidator
{
    public class UpdateBuildingRequestValidator : AbstractValidator<UpdateBuildingRequest>
    {
        public UpdateBuildingRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmptyWithException("Building name")
                .MaximumLength(100).WithMessage("Building name cannot exceed 100 characters");
        }
    }
}
