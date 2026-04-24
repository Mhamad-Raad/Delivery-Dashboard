using DeliveryDash.Domain.Entities;

namespace DeliveryDash.Application.Abstracts.IRepository
{
    public interface IOrderAssignmentRepository
    {
        Task<OrderAssignment?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<OrderAssignment?> GetPendingAssignmentForOrderAsync(int orderId, CancellationToken ct = default);
        Task<OrderAssignment?> GetAcceptedAssignmentAsync(int orderId, Guid driverId, CancellationToken ct = default);
        Task<OrderAssignment?> GetAcceptedAssignmentByOrderAsync(int orderId, CancellationToken ct = default);
        Task<IEnumerable<OrderAssignment>> GetExpiredAssignmentsAsync(CancellationToken ct = default);
        Task<OrderAssignment> CreateAsync(OrderAssignment assignment, CancellationToken ct = default);
        Task<OrderAssignment> UpdateAsync(OrderAssignment assignment, CancellationToken ct = default);
        Task<bool> HasAssignmentForDriverAsync(int orderId, Guid driverId, CancellationToken ct = default);
        Task<IEnumerable<int>> GetOrderIdsForDriverAsync(Guid driverId, CancellationToken ct = default);
        Task<int> GetAssignmentCountForOrderAsync(int orderId, CancellationToken ct = default);
    }
}