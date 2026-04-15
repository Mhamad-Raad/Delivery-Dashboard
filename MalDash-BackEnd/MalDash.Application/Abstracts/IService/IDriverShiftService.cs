using MalDash.Application.Responses.DriverResponses;

namespace MalDash.Application.Abstracts.IService
{
    public interface IDriverShiftService
    {
        Task<DriverShiftResponse> StartShiftAsync(Guid driverId, CancellationToken ct = default);
        Task<DriverShiftResponse> EndShiftAsync(Guid driverId, CancellationToken ct = default);
        Task<DriverShiftResponse?> GetActiveShiftAsync(Guid driverId, CancellationToken ct = default);
        Task<bool> IsDriverOnShiftAsync(Guid driverId, CancellationToken ct = default);
    }
}