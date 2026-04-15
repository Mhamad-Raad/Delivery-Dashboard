using MalDash.Application.Abstracts.IRepository;
using MalDash.Application.Abstracts.IService;
using MalDash.Application.Responses.DriverResponses;
using MalDash.Domain.Entities;

namespace MalDash.Application.Services
{
    public class DriverShiftService : IDriverShiftService
    {
        private readonly IDriverShiftRepository _shiftRepository;
        private readonly IDriverQueueService _queueService;

        public DriverShiftService(
            IDriverShiftRepository shiftRepository,
            IDriverQueueService queueService)
        {
            _shiftRepository = shiftRepository;
            _queueService = queueService;
        }

        public async Task<DriverShiftResponse> StartShiftAsync(Guid driverId, CancellationToken ct = default)
        {
            // Check for existing active shift
            var existingShift = await _shiftRepository.GetActiveShiftAsync(driverId, ct);
            if (existingShift != null)
            {
                throw new InvalidOperationException("Driver already has an active shift");
            }

            var shift = new DriverShift
            {
                DriverId = driverId,
                StartedAt = DateTime.UtcNow
            };

            var createdShift = await _shiftRepository.CreateAsync(shift, ct);

            // Add driver to the queue
            await _queueService.EnqueueDriverAsync(driverId, createdShift.Id, ct);

            return MapToResponse(createdShift);
        }

        public async Task<DriverShiftResponse> EndShiftAsync(Guid driverId, CancellationToken ct = default)
        {
            var shift = await _shiftRepository.GetActiveShiftAsync(driverId, ct)
                ?? throw new InvalidOperationException("No active shift found");

            shift.EndedAt = DateTime.UtcNow;
            var updatedShift = await _shiftRepository.UpdateAsync(shift, ct);

            // Remove driver from queue
            await _queueService.RemoveDriverAsync(driverId, ct);

            return MapToResponse(updatedShift);
        }

        public async Task<DriverShiftResponse?> GetActiveShiftAsync(Guid driverId, CancellationToken ct = default)
        {
            var shift = await _shiftRepository.GetActiveShiftAsync(driverId, ct);
            return shift != null ? MapToResponse(shift) : null;
        }

        public async Task<bool> IsDriverOnShiftAsync(Guid driverId, CancellationToken ct = default)
        {
            var shift = await _shiftRepository.GetActiveShiftAsync(driverId, ct);
            return shift != null;
        }

        private static DriverShiftResponse MapToResponse(DriverShift shift)
        {
            return new DriverShiftResponse
            {
                Id = shift.Id,
                DriverId = shift.DriverId,
                StartedAt = shift.StartedAt,
                EndedAt = shift.EndedAt,
                IsActive = shift.IsActive
            };
        }
    }
}