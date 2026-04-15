namespace MalDash.Application.Abstracts.IRepository
{
    public interface IDashboardRepository
    {
        // Admin dashboard methods
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveVendorsCountAsync();
        Task<int> GetTotalBuildingsCountAsync();
        Task<int> GetTotalApartmentsCountAsync();
        Task<int> GetOccupiedApartmentsCountAsync();
        Task<int> GetTotalProductsCountAsync();
        Task<int> GetPendingSupportRequestsCountAsync();

        // Vendor dashboard methods
        Task<int> GetVendorTotalOrdersAsync(int vendorId);
        Task<int> GetVendorPendingOrdersAsync(int vendorId);
        Task<int> GetVendorCompletedOrdersAsync(int vendorId);
        Task<int> GetVendorTotalProductsAsync(int vendorId);
        Task<decimal> GetVendorTotalRevenueAsync(int vendorId);
        Task<decimal> GetVendorTodayRevenueAsync(int vendorId);
        Task<int> GetVendorTodayOrdersAsync(int vendorId);
        Task<List<(DateTime Date, int Orders, decimal Revenue)>> GetVendorDailyStatsAsync(int vendorId, int days = 30);
    }
}