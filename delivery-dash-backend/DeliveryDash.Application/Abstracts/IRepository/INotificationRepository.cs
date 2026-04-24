using DeliveryDash.Application.Responses.NotificationResponses;
using DeliveryDash.Domain.Entities;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<List<Notification>> GetByUserIdAsync(Guid userId, int skip, int take);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<Notification?> GetByIdAsync(int id);
    Task UpdateAsync(Notification notification);
    Task DeleteAsync(Notification notification);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteOldNotificationsAsync(Guid userId, int maxCount);

    Task<List<BroadcastSummaryResponse>> GetBroadcastsAsync(int skip, int take);
    Task<BroadcastSummaryResponse?> GetBroadcastByKeyAsync(int key);
    Task<int> DeleteBroadcastByKeyAsync(int key);
}