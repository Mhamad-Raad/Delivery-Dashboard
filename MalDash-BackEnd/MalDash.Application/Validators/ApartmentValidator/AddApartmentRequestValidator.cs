using FluentValidation;
using MalDash.Application.Requests.ApartmentRequests;

namespace MalDash.Application.Validators.ApartmentValidator
{
    public class AddApartmentRequestValidator : AbstractValidator<AddApartmentRequest>
    {
        public AddApartmentRequestValidator()
        {
            RuleFor(x => x.ApartmentName)
                .NotEmptyWithException("Apartment name")
                .MaximumLength(50).WithMessage("Apartment name cannot exceed 50 characters");
        }
    }
}