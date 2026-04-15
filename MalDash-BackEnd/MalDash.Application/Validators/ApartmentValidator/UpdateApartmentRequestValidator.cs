using FluentValidation;
using MalDash.Application.Requests.ApartmentRequests;

namespace MalDash.Application.Validators.ApartmentValidator
{
    public class UpdateApartmentRequestValidator : AbstractValidator<UpdateApartmentRequest>
    {
        public UpdateApartmentRequestValidator()
        {
            RuleFor(x => x.ApartmentName)
                .NotEmptyWithException("Apartment name")
                .MaximumLength(50).WithMessage("Apartment name cannot exceed 50 characters");

            RuleFor(x => x.Layout)
                .SetValidator(new ApartmentLayoutRequestValidator()!)
                .When(x => x.Layout != null);
        }
    }
}