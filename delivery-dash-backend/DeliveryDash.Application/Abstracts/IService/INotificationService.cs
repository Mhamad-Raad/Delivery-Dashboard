using DeliveryDash.Application.Responses.NotificationResponses;

namespace DeliveryDash.Application.Abstracts.IService
{
    public enum BroadcastAudience
    {
        AllCustomers = 0,
        SpecificUsers = 1,
    }

    public interface INotificationService
    {
        Task SendNotificationAsync(
            Guid userId,
            string title,
            string message,
            string type = "Info",
            string? actionUrl = null,
            string? metadata = null,
            string? imageUrl = null);

        Task<List<NotificationResponse>> GetUserNotificationsAsync(Guid userId, int skip = 0, int take = 20);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(int notificationId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
        Task DeleteNotificationAsync(int notificationId, Guid userId);

        /// <summary>
        /// Persists + pushes a broadcast notification to the chosen audience.
        /// Returns the number of users that were targeted.
        /// </summary>
        Task<int> BroadcastAsync(
            string title,
            string message,
            string? imageUrl,
            BroadcastAudience audience,
            IEnumerable<Guid>? specificUserIds,
            CancellationToken ct = default);

        Task<List<BroadcastSummaryResponse>> GetBroadcastsAsync(int skip, int take);
        Task<BroadcastSummaryResponse?> GetBroadcastAsync(int key);
        Task<int> DeleteBroadcastAsync(int key);
    }
}
