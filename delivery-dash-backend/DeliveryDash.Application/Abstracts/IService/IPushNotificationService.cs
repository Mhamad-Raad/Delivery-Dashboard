namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IPushNotificationService
    {
        /// <summary>
        /// Sends a push notification to every device token registered for the given users.
        /// Silent no-op if Firebase is not configured. Invalid tokens are pruned.
        /// </summary>
        Task SendToUsersAsync(
            IEnumerable<Guid> userIds,
            string title,
            string body,
            string? imageUrl = null,
            IDictionary<string, string>? data = null,
            CancellationToken ct = default);

        /// <summary>
        /// Sends a push notification directly to a set of device tokens. Prunes bad tokens.
        /// </summary>
        Task SendToTokensAsync(
            IEnumerable<string> tokens,
            string title,
            string body,
            string? imageUrl = null,
            IDictionary<string, string>? data = null,
            CancellationToken ct = default);
    }
}
