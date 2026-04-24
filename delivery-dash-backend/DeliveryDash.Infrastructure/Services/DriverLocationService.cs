using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Responses.TrackingResponses;

namespace DeliveryDash.Infrastructure.Services
{
    public class DriverLocationService : IDriverLocationService
    {
        private static readonly TimeSpan Ttl = TimeSpan.FromHours(6);
        private readonly ICacheService _cache;

        public DriverLocationService(ICacheService cache)
        {
            _cache = cache;
        }

        public Task SetAsync(int orderId, double lat, double lng, double? heading, CancellationToken ct = default)
        {
            var payload = new DriverLocationResponse
            {
                OrderId = orderId,
                Lat = lat,
                Lng = lng,
                Heading = heading,
                RecordedAt = DateTime.UtcNow,
            };
            return _cache.SetAsync(Key(orderId), payload, Ttl, ct);
        }

        public Task<DriverLocationResponse?> GetAsync(int orderId, CancellationToken ct = default)
        {
            return _cache.GetAsync<DriverLocationResponse>(Key(orderId), ct);
        }

        public Task ClearAsync(int orderId, CancellationToken ct = default)
        {
            return _cache.RemoveAsync(Key(orderId), ct);
        }

        private static string Key(int orderId) => $"driver-loc:{orderId}";
    }
}
