using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Options;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Options;

namespace DeliveryDash.API.Services
{
    public class FirebasePushNotificationService : IPushNotificationService
    {
        private readonly IDeviceTokenRepository _tokens;
        private readonly ILogger<FirebasePushNotificationService> _logger;
        private readonly bool _enabled;

        public FirebasePushNotificationService(
            IDeviceTokenRepository tokens,
            IOptions<FirebaseOptions> options,
            ILogger<FirebasePushNotificationService> logger)
        {
            _tokens = tokens;
            _logger = logger;
            _enabled = !string.IsNullOrWhiteSpace(options.Value.ServiceAccountPath)
                && FirebaseAdmin.FirebaseApp.DefaultInstance is not null;
        }

        public async Task SendToUsersAsync(
            IEnumerable<Guid> userIds,
            string title,
            string body,
            string? imageUrl = null,
            IDictionary<string, string>? data = null,
            CancellationToken ct = default)
        {
            if (!_enabled)
            {
                _logger.LogDebug("Push disabled; skipping SendToUsersAsync");
                return;
            }

            var tokens = await _tokens.GetTokensForUsersAsync(userIds, ct);
            if (tokens.Count == 0) return;

            await SendToTokensAsync(tokens, title, body, imageUrl, data, ct);
        }

        public async Task SendToTokensAsync(
            IEnumerable<string> tokens,
            string title,
            string body,
            string? imageUrl = null,
            IDictionary<string, string>? data = null,
            CancellationToken ct = default)
        {
            if (!_enabled) return;

            var tokenList = tokens.Distinct().ToList();
            if (tokenList.Count == 0) return;

            var notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = title,
                Body = body,
                ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl,
            };

            var badTokens = new List<string>();

            // FCM multicast caps at 500 tokens per call.
            const int chunkSize = 500;
            for (var offset = 0; offset < tokenList.Count; offset += chunkSize)
            {
                var chunk = tokenList.Skip(offset).Take(chunkSize).ToList();
                var message = new MulticastMessage
                {
                    Tokens = chunk,
                    Notification = notification,
                    Data = data?.ToDictionary(kv => kv.Key, kv => kv.Value),
                };

                try
                {
                    var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message, ct);

                    if (response.FailureCount > 0)
                    {
                        for (var i = 0; i < response.Responses.Count; i++)
                        {
                            var r = response.Responses[i];
                            if (!r.IsSuccess && IsInvalidTokenError(r.Exception))
                            {
                                badTokens.Add(chunk[i]);
                            }
                        }
                    }

                    _logger.LogInformation(
                        "FCM multicast: sent={Success} failed={Failed} total={Total}",
                        response.SuccessCount, response.FailureCount, chunk.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FCM multicast failed for chunk of size {Size}", chunk.Count);
                }
            }

            if (badTokens.Count > 0)
            {
                _logger.LogInformation("Pruning {Count} invalid FCM tokens", badTokens.Count);
                await _tokens.RemoveTokensAsync(badTokens, ct);
            }
        }

        private static bool IsInvalidTokenError(FirebaseAdmin.Messaging.FirebaseMessagingException? ex)
        {
            if (ex is null) return false;
            return ex.MessagingErrorCode is MessagingErrorCode.Unregistered
                or MessagingErrorCode.InvalidArgument
                or MessagingErrorCode.SenderIdMismatch;
        }
    }
}
