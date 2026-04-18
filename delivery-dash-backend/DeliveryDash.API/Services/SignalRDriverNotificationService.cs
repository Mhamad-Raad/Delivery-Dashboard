using DeliveryDash.API.Hubs;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace DeliveryDash.API.Services
{
    public class SignalRDriverNotificationService : IDriverNotificationService
    {
        private readonly IHubContext<DispatchHub> _hubContext;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<SignalRDriverNotificationService> _logger;

        public SignalRDriverNotificationService(
            IHubContext<DispatchHub> hubContext,
            INotificationRepository notificationRepository,
            ILogger<SignalRDriverNotificationService> logger)
        {
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task SendOrderOfferAsync(
            Guid driverId,
            int assignmentId,
            Order order,
            int timeoutSeconds,
            CancellationToken ct = default)
        {
            var payload = new
            {
                AssignmentId = assignmentId,
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                VendorName = order.Vendor?.Name,
                DeliveryAddress = order.DeliveryAddress != null ? new
                {
                    Building = order.DeliveryAddress.Building?.Name,
                    Floor = order.DeliveryAddress.Floor?.FloorNumber,
                    Apartment = order.DeliveryAddress.Apartment?.ApartmentName
                } : null,
                TotalAmount = order.TotalAmount,
                TimeoutSeconds = timeoutSeconds,
                ExpiresAt = DateTime.UtcNow.AddSeconds(timeoutSeconds)
            };

            // Send real-time notification via SignalR
            await _hubContext.Clients
                .Group($"driver:{driverId}")
                .SendAsync("OrderOffer", payload, ct);

            // Persist notification directly to database
            try
            {
                var notification = new Notification
                {
                    UserId = driverId,
                    Title = "New Delivery Offer",
                    Message = $"Order #{order.OrderNumber} is available for delivery. Respond within {timeoutSeconds} seconds.",
                    Type = "DriverOffer",
                    ActionUrl = $"/driver/assignments/{assignmentId}",
                    Metadata = $"{{\"assignmentId\":{assignmentId},\"orderId\":{order.Id},\"timeoutSeconds\":{timeoutSeconds}}}",
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.CreateAsync(notification);
                _logger.LogInformation("Driver notification persisted for driver {DriverId}, order {OrderId}", driverId, order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist driver notification for driver {DriverId}", driverId);
            }
        }

        public async Task SendOrderCancelledAsync(Guid driverId, int orderId, CancellationToken ct = default)
        {
            await _hubContext.Clients
                .Group($"driver:{driverId}")
                .SendAsync("OrderCancelled", new { OrderId = orderId }, ct);

            // Persist cancellation notification directly to database
            try
            {
                var notification = new Notification
                {
                    UserId = driverId,
                    Title = "Order Cancelled",
                    Message = $"Order #{orderId} has been cancelled and is no longer available.",
                    Type = "DriverOffer",
                    ActionUrl = null,
                    Metadata = $"{{\"orderId\":{orderId}}}",
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.CreateAsync(notification);
                _logger.LogInformation("Driver cancellation notification persisted for driver {DriverId}, order {OrderId}", driverId, orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist driver cancellation notification for driver {DriverId}", driverId);
            }
        }
    }
}