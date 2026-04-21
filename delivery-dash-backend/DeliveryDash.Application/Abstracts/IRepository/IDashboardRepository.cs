using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Abstracts.IRepository
{
    public interface IDashboardRepository
    {
        // Admin dashboard totals (Home page)
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveVendorsCountAsync();
        Task<int> GetTotalProductsCountAsync();
        Task<int> GetPendingSupportRequestsCountAsync();

        // Vendor dashboard
        Task<int> GetVendorTotalOrdersAsync(int vendorId);
        Task<int> GetVendorPendingOrdersAsync(int vendorId);
        Task<int> GetVendorCompletedOrdersAsync(int vendorId);
        Task<int> GetVendorTotalProductsAsync(int vendorId);
        Task<decimal> GetVendorTotalRevenueAsync(int vendorId);
        Task<decimal> GetVendorTodayRevenueAsync(int vendorId);
        Task<int> GetVendorTodayOrdersAsync(int vendorId);
        Task<List<(DateTime Date, int Orders, decimal Revenue)>> GetVendorDailyStatsAsync(int vendorId, int days = 30);

        // Admin analytics (Reports page)
        Task<List<(DateTime Bucket, int Orders, decimal Revenue)>> GetOrderTimeSeriesAsync(DateTime fromUtc, DateTime toUtc, string granularity);
        Task<(int Total, int Delivered, int Cancelled, decimal Revenue, decimal Gmv)> GetOrderTotalsAsync(DateTime fromUtc, DateTime toUtc);
        Task<List<(OrderStatus Status, int Count)>> GetOrderStatusBreakdownAsync(DateTime fromUtc, DateTime toUtc);
        Task<List<(string CategoryName, decimal Revenue)>> GetRevenueByVendorCategoryAsync(DateTime fromUtc, DateTime toUtc);

        Task<List<(int VendorId, string Name, string CategoryName, int OrderCount, decimal Revenue)>> GetTopVendorsByRevenueAsync(DateTime fromUtc, DateTime toUtc, int take);
        Task<List<(int VendorId, string Name, string CategoryName, int OrderCount, decimal Revenue)>> GetTopVendorsByOrdersAsync(DateTime fromUtc, DateTime toUtc, int take);
        Task<int> GetVendorSignupsCountAsync(DateTime fromUtc, DateTime toUtc);
        Task<(int Active, int Inactive)> GetVendorActivityCountsAsync(DateTime fromUtc, DateTime toUtc);

        Task<List<(Guid DriverId, string Name, int Deliveries, double AvgMinutes)>> GetTopDriversByDeliveriesAsync(DateTime fromUtc, DateTime toUtc, int take);
        Task<double> GetAvgDeliveryMinutesAsync(DateTime fromUtc, DateTime toUtc);
        Task<int> GetActiveDriversCountAsync(DateTime fromUtc, DateTime toUtc);

        Task<int> GetCustomerSignupsCountAsync(DateTime fromUtc, DateTime toUtc);
        Task<List<(DateTime Bucket, int Count)>> GetCustomerSignupSeriesAsync(DateTime fromUtc, DateTime toUtc, string granularity);
        Task<List<(Guid CustomerId, string Name, int OrderCount, decimal TotalSpent)>> GetTopSpendersAsync(DateTime fromUtc, DateTime toUtc, int take);
        Task<(int Returning, int OneTime)> GetReturningVsOneTimeCustomersAsync(DateTime fromUtc, DateTime toUtc);

        Task<(int Opened, int Resolved, int OpenBacklog, double AvgResolutionHours)> GetSupportKpisAsync(DateTime fromUtc, DateTime toUtc);
    }
}
