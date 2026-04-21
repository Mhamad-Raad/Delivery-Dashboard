using DeliveryDash.Application.Responses.DashboardResponses;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IDashboardService
    {
        Task<AdminDashboardResponse> GetAdminDashboardAsync();
        Task InvalidateAdminDashboardCacheAsync();
        Task<VendorDashboardResponse> GetVendorDashboardAsync(int vendorId);
        Task<AdminAnalyticsResponse> GetAdminAnalyticsAsync(DateTime fromUtc, DateTime toUtc, string granularity);
    }
}