using DeliveryDash.Application.Responses.DriverResponses;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IDriverShiftService
    {
        Task<DriverShiftResponse> StartShiftAsync(Guid driverId, CancellationToken ct = default);
        Task<DriverShiftResponse> EndShiftAsync(Guid driverId, CancellationToken ct = default);
        Task<DriverShiftResponse?> GetActiveShiftAsync(Guid driverId, CancellationToken ct = default);
        Task<bool> IsDriverOnShiftAsync(Guid driverId, CancellationToken ct = default);
    }
}