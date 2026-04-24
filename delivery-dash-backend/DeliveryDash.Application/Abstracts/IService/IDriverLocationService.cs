using DeliveryDash.Application.Responses.TrackingResponses;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IDriverLocationService
    {
        Task SetAsync(int orderId, double lat, double lng, double? heading, CancellationToken ct = default);
        Task<DriverLocationResponse?> GetAsync(int orderId, CancellationToken ct = default);
        Task ClearAsync(int orderId, CancellationToken ct = default);
    }
}
