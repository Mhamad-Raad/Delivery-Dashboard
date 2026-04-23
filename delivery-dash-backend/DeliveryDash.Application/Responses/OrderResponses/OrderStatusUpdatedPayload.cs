using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Responses.OrderResponses
{
    public record OrderStatusUpdatedPayload(
        int OrderId,
        string OrderNumber,
        OrderStatus Status,
        OrderStatus? PreviousStatus,
        DateTime Timestamp,
        string TriggeredBy);
}
