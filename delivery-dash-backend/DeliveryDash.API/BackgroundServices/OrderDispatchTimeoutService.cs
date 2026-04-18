using DeliveryDash.Application.Abstracts.IService;

namespace DeliveryDash.API.BackgroundServices
{
    public class OrderDispatchTimeoutService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderDispatchTimeoutService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);

        public OrderDispatchTimeoutService(
            IServiceProvider serviceProvider,
            ILogger<OrderDispatchTimeoutService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order dispatch timeout service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dispatchService = scope.ServiceProvider
                        .GetRequiredService<IOrderDispatchService>();

                    await dispatchService.ProcessExpiredAssignmentsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing expired assignments");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}