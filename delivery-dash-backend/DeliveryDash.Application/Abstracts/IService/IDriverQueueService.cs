using DeliveryDash.Domain.Models;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IDriverQueueService
    {
        /// <summary>
        /// Adds a driver to the end of the queue.
        /// </summary>
        Task EnqueueDriverAsync(Guid driverId, int shiftId, CancellationToken ct = default);

        /// <summary>
        /// Removes and returns the first available driver from the queue.
        /// Uses distributed locking for concurrency safety.
        /// </summary>
        Task<DriverQueueEntry?> DequeueDriverAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns the driver to the end of the queue (after rejection/timeout).
        /// </summary>
        Task RequeueDriverAsync(Guid driverId, int shiftId, CancellationToken ct = default);

        /// <summary>
        /// Removes a driver from the queue (shift ended or went offline).
        /// </summary>
        Task RemoveDriverAsync(Guid driverId, CancellationToken ct = default);

        /// <summary>
        /// Checks if a driver is currently in the queue.
        /// </summary>
        Task<bool> IsDriverInQueueAsync(Guid driverId, CancellationToken ct = default);

        /// <summary>
        /// Gets the current queue position for a driver (1-based).
        /// </summary>
        Task<int?> GetDriverPositionAsync(Guid driverId, CancellationToken ct = default);

        /// <summary>
        /// Gets all drivers currently in the queue (for admin monitoring).
        /// </summary>
        Task<IReadOnlyList<DriverQueueEntry>> GetQueueSnapshotAsync(CancellationToken ct = default);

        /// <summary>
        /// Gets the total number of drivers in the queue.
        /// </summary>
        Task<int> GetQueueLengthAsync(CancellationToken ct = default);
    }
}