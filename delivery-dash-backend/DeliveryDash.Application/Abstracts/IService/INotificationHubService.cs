using DeliveryDash.Application.Responses.OrderResponses;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface INotificationHubService
    {
        Task SendNotificationToUserAsync(Guid userId, object notification);
        Task BroadcastOrderStatusAsync(OrderStatusUpdatedPayload payload, Guid customerId, Guid vendorOwnerUserId, Guid? driverUserId = null);
    }
}