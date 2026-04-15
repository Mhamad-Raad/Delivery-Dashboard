using FluentValidation;
using MalDash.Application.Requests.OrderRequests;

namespace MalDash.Application.Validators.OrderValidators
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.VendorId)
                .GreaterThan(0)
                .WithMessage("Vendor ID must be valid");

            RuleFor(x => x.DeliveryFee)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Delivery fee cannot be negative");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Order must contain at least one item");

            RuleForEach(x => x.Items).SetValidator(new OrderItemRequestValidator());
        }
    }

    public class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
    {
        public OrderItemRequestValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("Product ID must be valid");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be at least 1");
        }
    }
}