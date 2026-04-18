using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Responses.NotificationResponses;
using DeliveryDash.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace DeliveryDash.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationHubService? _hubService;
        private readonly IConfiguration _configuration;

        public NotificationService(
            INotificationRepository notificationRepository,
            IConfiguration configuration,
            INotificationHubService? hubService = null)
        {
            _notificationRepository = notificationRepository;
            _hubService = hubService;
            _configuration = configuration;
        }

        public async Task SendNotificationAsync(Guid userId, string title, string message, string type = "Info", string? actionUrl = null, string? metadata = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                ActionUrl = actionUrl,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow
            };

            var createdNotification = await _notificationRepository.CreateAsync(notification);

            // Check if real-time notifications are enabled AND hub service is available
            var enableRealTime = bool.Parse(_configuration["NotificationSettings:EnableRealTimeNotifications"] ?? "true");

            if (enableRealTime && _hubService != null)
            {
                // Send real-time notification via abstraction
                await _hubService.SendNotificationToUserAsync(userId, new
                {
                    createdNotification.Id,
                    createdNotification.Title,
                    createdNotification.Message,
                    createdNotification.Type,
                    createdNotification.ActionUrl,
                    createdNotification.Metadata,
                    createdNotification.CreatedAt
                });
            }

            // Cleanup old notifications
            var maxNotifications = int.Parse(_configuration["NotificationSettings:MaxNotificationsPerUser"] ?? "100");
            await _notificationRepository.DeleteOldNotificationsAsync(userId, maxNotifications);
        }

        public async Task<List<NotificationResponse>> GetUserNotificationsAsync(Guid userId, int skip = 0, int take = 20)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId, skip, take);
            return notifications.Select(n => new NotificationResponse
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ActionUrl = n.ActionUrl,
                Metadata = n.Metadata
            }).ToList();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task MarkAsReadAsync(int notificationId, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            if (notification != null && notification.UserId == userId)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task DeleteNotificationAsync(int notificationId, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            if (notification != null && notification.UserId == userId)
            {
                await _notificationRepository.DeleteAsync(notification);
            }
        }
    }
}