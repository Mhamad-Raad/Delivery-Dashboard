using MalDash.Domain.Entities;

namespace MalDash.Application.Abstracts.IRepository
{
    public interface IDriverShiftRepository
    {
        Task<DriverShift?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<DriverShift?> GetActiveShiftAsync(Guid driverId, CancellationToken ct = default);
        Task<IEnumerable<DriverShift>> GetShiftsByDriverIdAsync(Guid driverId, int page = 1, int limit = 10, CancellationToken ct = default);
        Task<DriverShift> CreateAsync(DriverShift shift, CancellationToken ct = default);
        Task<DriverShift> UpdateAsync(DriverShift shift, CancellationToken ct = default);
    }
}