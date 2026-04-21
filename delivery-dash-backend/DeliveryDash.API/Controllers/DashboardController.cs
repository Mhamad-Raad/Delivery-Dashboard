using System.Globalization;
using System.Text;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeliveryDash.Infrastructure;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ApplicationDbContext _db;

        public DashboardController(IDashboardService dashboardService, ApplicationDbContext db)
        {
            _dashboardService = dashboardService;
            _db = db;
        }

        [HttpGet("admin")]
        [EndpointDescription("Retrieves high-level dashboard totals for administrators: total users, active vendors, total products, and pending support requests. Cached. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var dashboard = await _dashboardService.GetAdminDashboardAsync();
            return Ok(dashboard);
        }

        [HttpPost("admin/invalidate-cache")]
        [EndpointDescription("Invalidates the admin dashboard cache to force fresh data retrieval on the next request. Useful after bulk operations that significantly change dashboard metrics. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> InvalidateAdminDashboardCache()
        {
            await _dashboardService.InvalidateAdminDashboardCacheAsync();
            return Ok(new { message = "Admin dashboard cache invalidated successfully" });
        }

        [HttpGet("admin/analytics")]
        [EndpointDescription("Returns admin-facing analytics for the Reports page: financial (revenue, GMV, AOV, series, by-category), orders (totals, series, status breakdown, cancellation rate), vendors (new signups, active/inactive, top by revenue/orders), drivers (active count, avg delivery time, top by deliveries), customers (signups, returning vs one-time, top spenders), and support KPIs (opened/resolved/backlog/avg resolution hours). Aggregated over the given UTC date range at day/week/month granularity. Cached per range+granularity. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> GetAdminAnalytics(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string granularity = "day")
        {
            if (to < from)
                return BadRequest(new { message = "'to' must be >= 'from'." });

            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            var result = await _dashboardService.GetAdminAnalyticsAsync(fromUtc, toUtc, granularity);
            return Ok(result);
        }

        [HttpGet("admin/export/orders.csv")]
        [EndpointDescription("Streams a CSV export of orders in the given UTC date range, with optional status and vendor filters. Columns: OrderNumber, CreatedAt, CompletedAt, VendorId, VendorName, CustomerId, Status, Subtotal, DeliveryFee, TotalAmount. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> ExportOrdersCsv(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] OrderStatus? status = null,
            [FromQuery] int? vendorId = null)
        {
            if (to < from)
                return BadRequest(new { message = "'to' must be >= 'from'." });

            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            var query = _db.Orders
                .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc);

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            if (vendorId.HasValue)
                query = query.Where(o => o.VendorId == vendorId.Value);

            var rows = await query
                .OrderBy(o => o.CreatedAt)
                .Select(o => new
                {
                    o.OrderNumber,
                    o.CreatedAt,
                    o.CompletedAt,
                    o.VendorId,
                    VendorName = o.Vendor.Name,
                    o.UserId,
                    o.Status,
                    o.Subtotal,
                    o.DeliveryFee,
                    o.TotalAmount
                })
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("OrderNumber,CreatedAtUtc,CompletedAtUtc,VendorId,VendorName,CustomerId,Status,Subtotal,DeliveryFee,TotalAmount");
            var ci = CultureInfo.InvariantCulture;
            foreach (var r in rows)
            {
                sb.Append(Csv(r.OrderNumber)).Append(',');
                sb.Append(r.CreatedAt.ToString("o", ci)).Append(',');
                sb.Append(r.CompletedAt?.ToString("o", ci) ?? "").Append(',');
                sb.Append(r.VendorId).Append(',');
                sb.Append(Csv(r.VendorName)).Append(',');
                sb.Append(r.UserId).Append(',');
                sb.Append(r.Status).Append(',');
                sb.Append(r.Subtotal.ToString(ci)).Append(',');
                sb.Append(r.DeliveryFee.ToString(ci)).Append(',');
                sb.Append(r.TotalAmount.ToString(ci)).AppendLine();
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"orders_{fromUtc:yyyyMMdd}_{toUtc:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }

        private static string Csv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}