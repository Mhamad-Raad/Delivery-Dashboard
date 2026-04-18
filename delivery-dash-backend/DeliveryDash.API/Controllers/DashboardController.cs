using DeliveryDash.Application.Abstracts.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("admin")]
        [EndpointDescription("Retrieves comprehensive dashboard statistics for administrators. Returns metrics including total users, active vendors, buildings, apartments, occupied apartments, total products, and pending support requests. Results are cached for performance. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var dashboard = await _dashboardService.GetAdminDashboardAsync();
            return Ok(dashboard);
        }

        [HttpPost("admin/invalidate-cache")]
        [EndpointDescription("Invalidates the admin dashboard cache to force fresh data retrieval on the next request. Useful after bulk operations that significantly change dashboard metrics (e.g., data imports, mass deletions). Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> InvalidateAdminDashboardCache()
        {
            await _dashboardService.InvalidateAdminDashboardCacheAsync();
            return Ok(new { message = "Admin dashboard cache invalidated successfully" });
        }
    }
}