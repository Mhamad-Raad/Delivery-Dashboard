using DeliveryDash.API.BackgroundServices;
using DeliveryDash.API.Services;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Services;
using DeliveryDash.Infrastructure.Services;

namespace DeliveryDash.API.Extensions
{
    public static class DispatchServiceExtensions
    {
        public static IServiceCollection AddDriverDispatchServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure dispatch settings
            services.Configure<OrderDispatchSettings>(
                configuration.GetSection("OrderDispatch"));

            // Register dispatch services
            services.AddScoped<IDriverQueueService, RedisDriverQueueService>();
            services.AddScoped<IDriverShiftService, DriverShiftService>();
            services.AddScoped<IOrderDispatchService, OrderDispatchService>();
            
            // SignalR service lives in API layer (depends on Hub)
            services.AddScoped<IDriverNotificationService, SignalRDriverNotificationService>();

            // Background services for timeout handling
            services.AddHostedService<OrderDispatchTimeoutService>();
            services.AddHostedService<VendorResponseTimeoutService>();

            return services;
        }
    }
}