using FluentValidation;
using MalDash.Application.Requests.OrderRequests;
using MalDash.Domain.Enums;

namespace MalDash.Application.Validators.OrderValidators
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