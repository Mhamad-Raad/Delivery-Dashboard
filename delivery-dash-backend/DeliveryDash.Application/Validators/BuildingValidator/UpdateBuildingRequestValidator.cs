using FluentValidation;
using DeliveryDash.Application.Requests.BuildingRequests;

namespace DeliveryDash.Application.Validators.BuildingValidator
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
