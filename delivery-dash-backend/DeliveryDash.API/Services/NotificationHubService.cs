using DeliveryDash.API.Hubs;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Responses.OrderResponses;
using Microsoft.AspNetCore.SignalR;

namespace DeliveryDash.API.Services
{
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUserAsync(Guid userId, object notification)
        {
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
        }

        public async Task BroadcastOrderStatusAsync(OrderStatusUpdatedPayload payload, Guid customerId, Guid vendorOwnerUserId)
        {
            var groups = new[]
            {
                $"user_{customerId}",
                $"user_{vendorOwnerUserId}",
                "role_admin"
            };

            await _hubContext.Clients.Groups(groups).SendAsync("OrderStatusUpdated", payload);
        }
    }
}