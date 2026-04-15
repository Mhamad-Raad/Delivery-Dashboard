using MalDash.Application.Abstracts.IRepository;
using MalDash.Application.Abstracts.IService;
using MalDash.Application.Options;
using MalDash.Application.Responses.DashboardResponses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MalDash.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ICacheService _cacheService;
        private readonly DashboardCacheOptions _cacheOptions;
        private readonly ILogger<DashboardService> _logger;

        private const string ADMIN_DASHBOARD_CACHE_KEY = "dashboard:admin";
        private const string VENDOR_DASHBOARD_CACHE_KEY = "dashboard:vendor:";

        public DashboardService(
            IDashboardRepository dashboardRepository,
            ICacheService cacheService,
            IOptions<DashboardCacheOptions> cacheOptions,
            ILogger<DashboardService> logger)
        {
            _dashboardRepository = dashboardRepository;
            _cacheService = cacheService;
            _cacheOptions = cacheOptions.Value;
            _logger = logger;
        }

        public async Task<AdminDashboardResponse> GetAdminDashboardAsync()
        {
            var cachedData = await _cacheService.GetAsync<AdminDashboardResponse>(ADMIN_DASHBOARD_CACHE_KEY);

            if (cachedData != null)
            {
                _logger.LogInformation("Admin dashboard data retrieved from cache");
                return cachedData;
            }

            _logger.LogInformation("Admin dashboard cache miss, fetching from database");

            var dashboardData = new AdminDashboardResponse
            {
                TotalUsers = await _dashboardRepository.GetTotalUsersCountAsync(),
                ActiveVendors = await _dashboardRepository.GetActiveVendorsCountAsync(),
                TotalBuildings = await _dashboardRepository.GetTotalBuildingsCountAsync(),
                TotalApartments = await _dashboardRepository.GetTotalApartmentsCountAsync(),
                OccupiedApartments = await _dashboardRepository.GetOccupiedApartmentsCountAsync(),
                TotalProducts = await _dashboardRepository.GetTotalProductsCountAsync(),
                PendingRequests = await _dashboardRepository.GetPendingSupportRequestsCountAsync()
            };

            var cacheDuration = TimeSpan.FromMinutes(_cacheOptions.AdminDashboardCacheDurationMinutes);
            await _cacheService.SetAsync(ADMIN_DASHBOARD_CACHE_KEY, dashboardData, cacheDuration);

            _logger.LogInformation("Admin dashboard data cached for {Duration} minutes",
                _cacheOptions.AdminDashboardCacheDurationMinutes);

            return dashboardData;
        }

        public async Task InvalidateAdminDashboardCacheAsync()
        {
            await _cacheService.RemoveAsync(ADMIN_DASHBOARD_CACHE_KEY);
            _logger.LogInformation("Admin dashboard cache invalidated");
        }

        public async Task<VendorDashboardResponse> GetVendorDashboardAsync(int vendorId)
        {
            var cacheKey = $"{VENDOR_DASHBOARD_CACHE_KEY}{vendorId}";
            var cachedData = await _cacheService.GetAsync<VendorDashboardResponse>(cacheKey);

            if (cachedData != null)
            {
                _logger.LogInformation("Vendor dashboard data retrieved from cache for vendor {VendorId}", vendorId);
                return cachedData;
            }

            _logger.LogInformation("Vendor dashboard cache miss, fetching from database for vendor {VendorId}", vendorId);

            var dailyStats = await _dashboardRepository.GetVendorDailyStatsAsync(vendorId, 30);

            var dashboardData = new VendorDashboardResponse
            {
                TotalOrders = await _dashboardRepository.GetVendorTotalOrdersAsync(vendorId),
                PendingOrders = await _dashboardRepository.GetVendorPendingOrdersAsync(vendorId),
                CompletedOrders = await _dashboardRepository.GetVendorCompletedOrdersAsync(vendorId),
                TotalProducts = await _dashboardRepository.GetVendorTotalProductsAsync(vendorId),
                TotalRevenue = await _dashboardRepository.GetVendorTotalRevenueAsync(vendorId),
                TodayRevenue = await _dashboardRepository.GetVendorTodayRevenueAsync(vendorId),
                TodayOrders = await _dashboardRepository.GetVendorTodayOrdersAsync(vendorId),
                DailyStats = dailyStats.Select(s => new DailyOrderStatResponse
                {
                    Day = s.Date.ToString("MMM d"),
                    Orders = s.Orders,
                    Profit = s.Revenue
                }).ToList()
            };

            var cacheDuration = TimeSpan.FromMinutes(_cacheOptions.AdminDashboardCacheDurationMinutes);
            await _cacheService.SetAsync(cacheKey, dashboardData, cacheDuration);

            return dashboardData;
        }
    }
}