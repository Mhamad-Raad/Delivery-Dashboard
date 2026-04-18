using FluentValidation;
using DeliveryDash.Application.Requests.OrderRequests;
using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Validators.OrderValidators
{
    public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusRequest>
    {
        public UpdateOrderStatusValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid order status");
        }
    }
}