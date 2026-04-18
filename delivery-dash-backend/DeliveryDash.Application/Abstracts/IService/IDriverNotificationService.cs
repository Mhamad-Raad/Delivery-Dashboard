using DeliveryDash.Domain.Entities;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IDriverNotificationService
    {
        Task SendOrderOfferAsync(
            Guid driverId,
            int assignmentId,
            Order order,
            int timeoutSeconds,
            CancellationToken ct = default);

        Task SendOrderCancelledAsync(Guid driverId, int orderId, CancellationToken ct = default);
    }
}