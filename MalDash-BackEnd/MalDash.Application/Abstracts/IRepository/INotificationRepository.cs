using MalDash.Domain.Entities;

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
}