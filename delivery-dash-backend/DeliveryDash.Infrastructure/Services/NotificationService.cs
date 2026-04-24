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
        private readonly IDeviceTokenRepository _deviceTokenRepository;
        private readonly INotificationHubService? _hubService;
        private readonly IPushNotificationService? _pushService;
        private readonly IConfiguration _configuration;

        public NotificationService(
            INotificationRepository notificationRepository,
            IDeviceTokenRepository deviceTokenRepository,
            IConfiguration configuration,
            INotificationHubService? hubService = null,
            IPushNotificationService? pushService = null)
        {
            _notificationRepository = notificationRepository;
            _deviceTokenRepository = deviceTokenRepository;
            _hubService = hubService;
            _pushService = pushService;
            _configuration = configuration;
        }

        public async Task SendNotificationAsync(
            Guid userId,
            string title,
            string message,
            string type = "Info",
            string? actionUrl = null,
            string? metadata = null,
            string? imageUrl = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                ActionUrl = actionUrl,
                Metadata = metadata,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
            };

            var createdNotification = await _notificationRepository.CreateAsync(notification);

            // Realtime SignalR fan-out.
            var enableRealTime = bool.Parse(_configuration["NotificationSettings:EnableRealTimeNotifications"] ?? "true");
            if (enableRealTime && _hubService != null)
            {
                await _hubService.SendNotificationToUserAsync(userId, new
                {
                    createdNotification.Id,
                    createdNotification.Title,
                    createdNotification.Message,
                    createdNotification.Type,
                    createdNotification.ActionUrl,
                    createdNotification.Metadata,
                    createdNotification.ImageUrl,
                    createdNotification.CreatedAt,
                });
            }

            // FCM push fan-out (no-op if Firebase not configured).
            if (_pushService != null)
            {
                try
                {
                    var data = new Dictionary<string, string>
                    {
                        ["notificationId"] = createdNotification.Id.ToString(),
                        ["type"] = type,
                    };
                    if (!string.IsNullOrEmpty(actionUrl)) data["actionUrl"] = actionUrl;
                    if (!string.IsNullOrEmpty(metadata)) data["metadata"] = metadata;

                    await _pushService.SendToUsersAsync(new[] { userId }, title, message, imageUrl, data);
                }
                catch
                {
                    // Never let push failure block the DB/SignalR path.
                }
            }

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
                Metadata = n.Metadata,
                ImageUrl = n.ImageUrl,
            }).ToList();
        }

        public Task<int> GetUnreadCountAsync(Guid userId) =>
            _notificationRepository.GetUnreadCountAsync(userId);

        public async Task MarkAsReadAsync(int notificationId, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification != null && notification.UserId == userId)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public Task MarkAllAsReadAsync(Guid userId) =>
            _notificationRepository.MarkAllAsReadAsync(userId);

        public async Task DeleteNotificationAsync(int notificationId, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification != null && notification.UserId == userId)
            {
                await _notificationRepository.DeleteAsync(notification);
            }
        }

        public async Task<int> BroadcastAsync(
            string title,
            string message,
            string? imageUrl,
            BroadcastAudience audience,
            IEnumerable<Guid>? specificUserIds,
            CancellationToken ct = default)
        {
            List<Guid> targets = audience switch
            {
                BroadcastAudience.AllCustomers =>
                    await _deviceTokenRepository.GetAllCustomerUserIdsAsync(ct),
                BroadcastAudience.SpecificUsers =>
                    (specificUserIds ?? Enumerable.Empty<Guid>()).Distinct().ToList(),
                _ => new List<Guid>(),
            };

            if (targets.Count == 0) return 0;

            // Persist one row per target user so each user sees it in their notification list.
            var now = DateTime.UtcNow;
            foreach (var userId in targets)
            {
                await _notificationRepository.CreateAsync(new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = "Broadcast",
                    ImageUrl = imageUrl,
                    CreatedAt = now,
                });
            }

            // Realtime SignalR to whichever users are online.
            if (_hubService != null)
            {
                foreach (var userId in targets)
                {
                    await _hubService.SendNotificationToUserAsync(userId, new
                    {
                        Title = title,
                        Message = message,
                        Type = "Broadcast",
                        ImageUrl = imageUrl,
                        CreatedAt = now,
                    });
                }
            }

            // FCM push.
            if (_pushService != null)
            {
                try
                {
                    await _pushService.SendToUsersAsync(targets, title, message, imageUrl,
                        new Dictionary<string, string> { ["type"] = "Broadcast" }, ct);
                }
                catch
                {
                    // swallow; DB/SignalR already succeeded
                }
            }

            return targets.Count;
        }
    }
}
