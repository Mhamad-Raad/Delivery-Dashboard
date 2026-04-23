using DeliveryDash.Application.Abstracts;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Domain.Enums;

namespace DeliveryDash.API.BackgroundServices
{
    public class VendorResponseTimeoutService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VendorResponseTimeoutService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _vendorTimeout = TimeSpan.FromMinutes(2);

        public VendorResponseTimeoutService(
            IServiceProvider serviceProvider,
            ILogger<VendorResponseTimeoutService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Vendor response timeout service started with {Timeout} minute timeout", _vendorTimeout.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessTimedOutOrdersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing timed out orders");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessTimedOutOrdersAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var cutoffTime = DateTime.UtcNow - _vendorTimeout;

            // Get all pending orders that have exceeded the timeout
            var (pendingOrders, _) = await orderRepository.GetOrdersPagedAsync(
                page: 1,
                limit: 100,
                userId: null,
                vendorId: null,
                status: OrderStatus.Pending);

            var timedOutOrders = pendingOrders
                .Where(o => o.CreatedAt <= cutoffTime)
                .ToList();

            foreach (var order in timedOutOrders)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    var now = DateTime.UtcNow;

                    await using var tx = await unitOfWork.BeginTransactionAsync(stoppingToken);

                    order.Status = OrderStatus.Cancelled;
                    order.CancelledAt = now;
                    order.CompletedAt = now;

                    await orderRepository.UpdateAsync(order);

                    await notificationService.SendNotificationAsync(
                        order.UserId,
                        "Order Cancelled",
                        $"Your order #{order.OrderNumber} has been cancelled due to no response from the vendor. We apologize for the inconvenience.",
                        "Order",
                        $"/orders/{order.Id}",
                        $"{{\"orderId\":{order.Id},\"orderNumber\":\"{order.OrderNumber}\",\"reason\":\"VendorNoResponse\"}}");

                    await tx.CommitAsync(stoppingToken);

                    _logger.LogInformation(
                        "Order {OrderId} cancelled due to vendor timeout. Customer {UserId} notified.",
                        order.Id, order.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to cancel timed out order {OrderId}", order.Id);
                }
            }

            if (timedOutOrders.Count > 0)
            {
                _logger.LogInformation("Processed {Count} timed out orders", timedOutOrders.Count);
            }
        }
    }
}