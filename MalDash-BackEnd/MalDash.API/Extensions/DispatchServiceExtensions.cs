using MalDash.API.BackgroundServices;
using MalDash.API.Services;
using MalDash.Application.Abstracts.IService;
using MalDash.Application.Services;
using MalDash.Infrastructure.Services;

namespace MalDash.API.Extensions
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