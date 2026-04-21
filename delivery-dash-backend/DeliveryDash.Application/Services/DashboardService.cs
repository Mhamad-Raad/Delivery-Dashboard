using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Options;
using DeliveryDash.Application.Responses.DashboardResponses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeliveryDash.Application.Services
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

        public async Task<AdminAnalyticsResponse> GetAdminAnalyticsAsync(DateTime fromUtc, DateTime toUtc, string granularity)
        {
            granularity = string.IsNullOrWhiteSpace(granularity) ? "day" : granularity.ToLowerInvariant();
            if (granularity != "day" && granularity != "week" && granularity != "month")
                granularity = "day";

            var cacheKey = $"dashboard:admin:analytics:{fromUtc:yyyyMMddHHmm}:{toUtc:yyyyMMddHHmm}:{granularity}";
            var cached = await _cacheService.GetAsync<AdminAnalyticsResponse>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Admin analytics cache hit {Key}", cacheKey);
                return cached;
            }

            var ordersTotals = await _dashboardRepository.GetOrderTotalsAsync(fromUtc, toUtc);
            var ordersSeries = await _dashboardRepository.GetOrderTimeSeriesAsync(fromUtc, toUtc, granularity);
            var statusBreakdown = await _dashboardRepository.GetOrderStatusBreakdownAsync(fromUtc, toUtc);
            var revenueByCategory = await _dashboardRepository.GetRevenueByVendorCategoryAsync(fromUtc, toUtc);

            var topVendorsRevenue = await _dashboardRepository.GetTopVendorsByRevenueAsync(fromUtc, toUtc, 10);
            var topVendorsOrders = await _dashboardRepository.GetTopVendorsByOrdersAsync(fromUtc, toUtc, 10);
            var vendorSignups = await _dashboardRepository.GetVendorSignupsCountAsync(fromUtc, toUtc);
            var vendorActivity = await _dashboardRepository.GetVendorActivityCountsAsync(fromUtc, toUtc);

            var topDrivers = await _dashboardRepository.GetTopDriversByDeliveriesAsync(fromUtc, toUtc, 10);
            var avgDeliveryMinutes = await _dashboardRepository.GetAvgDeliveryMinutesAsync(fromUtc, toUtc);
            var activeDrivers = await _dashboardRepository.GetActiveDriversCountAsync(fromUtc, toUtc);

            var customerSignups = await _dashboardRepository.GetCustomerSignupsCountAsync(fromUtc, toUtc);
            var customerSignupSeries = await _dashboardRepository.GetCustomerSignupSeriesAsync(fromUtc, toUtc, granularity);
            var topSpenders = await _dashboardRepository.GetTopSpendersAsync(fromUtc, toUtc, 10);
            var returningVsOneTime = await _dashboardRepository.GetReturningVsOneTimeCustomersAsync(fromUtc, toUtc);

            var support = await _dashboardRepository.GetSupportKpisAsync(fromUtc, toUtc);

            var response = new AdminAnalyticsResponse
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                Granularity = granularity,
                Financial = new FinancialSection
                {
                    TotalRevenue = ordersTotals.Revenue,
                    Gmv = ordersTotals.Gmv,
                    AvgOrderValue = ordersTotals.Delivered == 0 ? 0 : ordersTotals.Revenue / ordersTotals.Delivered,
                    RevenueSeries = ordersSeries
                        .Select(s => new TimePoint { Bucket = s.Bucket, Value = s.Revenue })
                        .ToList(),
                    RevenueByCategory = revenueByCategory
                        .Select(r => new NamedAmount { Name = r.CategoryName, Amount = r.Revenue })
                        .ToList()
                },
                Orders = new OrdersSection
                {
                    Total = ordersTotals.Total,
                    CancellationRate = ordersTotals.Total == 0 ? 0 : Math.Round((decimal)ordersTotals.Cancelled / ordersTotals.Total, 4),
                    OrdersSeries = ordersSeries
                        .Select(s => new TimePoint { Bucket = s.Bucket, Value = s.Orders })
                        .ToList(),
                    StatusBreakdown = statusBreakdown
                        .Select(s => new StatusCount { Status = s.Status.ToString(), Count = s.Count })
                        .ToList()
                },
                Vendors = new VendorsSection
                {
                    NewSignups = vendorSignups,
                    ActiveCount = vendorActivity.Active,
                    InactiveCount = vendorActivity.Inactive,
                    TopByRevenue = topVendorsRevenue.Select(v => new VendorRanking
                    {
                        VendorId = v.VendorId,
                        Name = v.Name,
                        CategoryName = v.CategoryName,
                        OrderCount = v.OrderCount,
                        Revenue = v.Revenue
                    }).ToList(),
                    TopByOrders = topVendorsOrders.Select(v => new VendorRanking
                    {
                        VendorId = v.VendorId,
                        Name = v.Name,
                        CategoryName = v.CategoryName,
                        OrderCount = v.OrderCount,
                        Revenue = v.Revenue
                    }).ToList()
                },
                Drivers = new DriversSection
                {
                    ActiveCount = activeDrivers,
                    AvgDeliveryMinutes = avgDeliveryMinutes,
                    TopByDeliveries = topDrivers.Select(d => new DriverRanking
                    {
                        DriverId = d.DriverId,
                        Name = d.Name,
                        Deliveries = d.Deliveries,
                        AvgDeliveryMinutes = d.AvgMinutes
                    }).ToList()
                },
                Customers = new CustomersSection
                {
                    NewSignups = customerSignups,
                    Returning = returningVsOneTime.Returning,
                    OneTime = returningVsOneTime.OneTime,
                    SignupSeries = customerSignupSeries
                        .Select(s => new TimePoint { Bucket = s.Bucket, Value = s.Count })
                        .ToList(),
                    TopSpenders = topSpenders.Select(c => new CustomerRanking
                    {
                        CustomerId = c.CustomerId,
                        Name = c.Name,
                        OrderCount = c.OrderCount,
                        TotalSpent = c.TotalSpent
                    }).ToList()
                },
                Support = new SupportSection
                {
                    Opened = support.Opened,
                    Resolved = support.Resolved,
                    OpenBacklog = support.OpenBacklog,
                    AvgResolutionHours = support.AvgResolutionHours
                }
            };

            var cacheDuration = TimeSpan.FromMinutes(_cacheOptions.AdminDashboardCacheDurationMinutes);
            await _cacheService.SetAsync(cacheKey, response, cacheDuration);
            _logger.LogInformation("Admin analytics computed and cached {Key}", cacheKey);

            return response;
        }
    }
}