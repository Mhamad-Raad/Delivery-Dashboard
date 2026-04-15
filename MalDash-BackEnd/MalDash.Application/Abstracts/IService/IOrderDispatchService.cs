using MalDash.Application.Responses.OrderResponses;

namespace MalDash.Application.Abstracts.IService
{
    public interface IOrderDispatchService
    {
        /// <summary>
        /// Dispatches an order to the next available driver.
        /// </summary>
        Task<OrderAssignmentResponse?> DispatchOrderAsync(int orderId, CancellationToken ct = default);

        /// <summary>
        /// Driver accepts the order assignment.
        /// </summary>
        Task<OrderAssignmentResponse> AcceptOrderAsync(int assignmentId, Guid driverId, CancellationToken ct = default);

        /// <summary>
        /// Driver rejects the order assignment.
        /// </summary>
        Task<OrderAssignmentResponse> RejectOrderAsync(int assignmentId, Guid driverId, CancellationToken ct = default);

        /// <summary>
        /// Driver completes the delivery.
        /// </summary>
        Task CompleteDeliveryAsync(int orderId, Guid driverId, CancellationToken ct = default);

        /// <summary>
        /// Handles expired order assignments (called by background service).
        /// </summary>
        Task ProcessExpiredAssignmentsAsync(CancellationToken ct = default);
    }
}