using MalDash.Application.Responses.NotificationResponses;

namespace MalDash.Application.Abstracts.IService
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Guid userId, string title, string message, string type = "Info", string? actionUrl = null, string? metadata = null);
        Task<List<NotificationResponse>> GetUserNotificationsAsync(Guid userId, int skip = 0, int take = 20);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(int notificationId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
        Task DeleteNotificationAsync(int notificationId, Guid userId);
    }
}