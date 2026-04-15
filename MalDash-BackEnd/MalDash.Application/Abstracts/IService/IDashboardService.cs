using MalDash.Application.Responses.DashboardResponses;

namespace MalDash.Application.Abstracts.IService
{
    public interface IDashboardService
    {
        Task<AdminDashboardResponse> GetAdminDashboardAsync();
        Task InvalidateAdminDashboardCacheAsync();
        Task<VendorDashboardResponse> GetVendorDashboardAsync(int vendorId);
    }
}