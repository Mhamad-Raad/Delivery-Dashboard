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

        [HttpGet("admin/export/vendors.csv")]
        [EndpointDescription("Streams a CSV export of vendors that had at least one order in the given UTC window. Columns: VendorId, Name, Category, OrderCount, Revenue, CreatedAtUtc. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> ExportVendorsCsv([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (to < from) return BadRequest(new { message = "'to' must be >= 'from'." });

            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            var rows = await _db.Orders
                .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc)
                .GroupBy(o => o.VendorId)
                .Select(g => new
                {
                    VendorId = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Subtotal)
                })
                .Join(_db.Vendors, x => x.VendorId, v => v.Id,
                    (x, v) => new { x.VendorId, v.Name, v.VendorCategoryId, v.CreatedAt, x.OrderCount, x.Revenue })
                .Join(_db.VendorCategories, x => x.VendorCategoryId, vc => vc.Id,
                    (x, vc) => new { x.VendorId, x.Name, CategoryName = vc.Name, x.CreatedAt, x.OrderCount, x.Revenue })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("VendorId,Name,Category,OrderCount,Revenue,CreatedAtUtc");
            var ci = CultureInfo.InvariantCulture;
            foreach (var r in rows)
            {
                sb.Append(r.VendorId).Append(',');
                sb.Append(Csv(r.Name)).Append(',');
                sb.Append(Csv(r.CategoryName)).Append(',');
                sb.Append(r.OrderCount).Append(',');
                sb.Append(r.Revenue.ToString(ci)).Append(',');
                sb.Append(r.CreatedAt.ToString("o", ci)).AppendLine();
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                $"vendors_{fromUtc:yyyyMMdd}_{toUtc:yyyyMMdd}.csv");
        }

        [HttpGet("admin/export/drivers.csv")]
        [EndpointDescription("Streams a CSV export of drivers with completed deliveries in the given UTC window. Columns: DriverId, Name, Deliveries, AvgDeliveryMinutes. Delivery time is measured from driver acceptance (RespondedAt) to order completion (CompletedAt). Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> ExportDriversCsv([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (to < from) return BadRequest(new { message = "'to' must be >= 'from'." });

            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            var raw = await _db.OrderAssignments
                .Where(a => a.Status == OrderAssignmentStatus.Accepted &&
                            a.Order.Status == OrderStatus.Delivered &&
                            a.Order.CompletedAt.HasValue &&
                            a.Order.CompletedAt >= fromUtc &&
                            a.Order.CompletedAt <= toUtc &&
                            a.RespondedAt.HasValue)
                .Select(a => new
                {
                    a.DriverId,
                    Name = a.Driver.FirstName + " " + a.Driver.LastName,
                    RespondedAt = a.RespondedAt!.Value,
                    CompletedAt = a.Order.CompletedAt!.Value
                })
                .ToListAsync();

            var grouped = raw
                .GroupBy(r => new { r.DriverId, r.Name })
                .Select(g => new
                {
                    g.Key.DriverId,
                    g.Key.Name,
                    Deliveries = g.Count(),
                    AvgMinutes = g.Average(r => (r.CompletedAt - r.RespondedAt).TotalMinutes)
                })
                .OrderByDescending(x => x.Deliveries)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("DriverId,Name,Deliveries,AvgDeliveryMinutes");
            var ci = CultureInfo.InvariantCulture;
            foreach (var r in grouped)
            {
                sb.Append(r.DriverId).Append(',');
                sb.Append(Csv(r.Name)).Append(',');
                sb.Append(r.Deliveries).Append(',');
                sb.Append(Math.Round(r.AvgMinutes, 2).ToString(ci)).AppendLine();
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                $"drivers_{fromUtc:yyyyMMdd}_{toUtc:yyyyMMdd}.csv");
        }

        [HttpGet("admin/export/customers.csv")]
        [EndpointDescription("Streams a CSV export of customers with at least one order in the given UTC window. Columns: CustomerId, Name, Email, OrderCount, TotalSpent. TotalSpent is the sum of TotalAmount across delivered orders only. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> ExportCustomersCsv([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (to < from) return BadRequest(new { message = "'to' must be >= 'from'." });

            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            var rows = await _db.Orders
                .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc)
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    OrderCount = g.Count(),
                    TotalSpent = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount)
                })
                .Join(_db.Users, x => x.UserId, u => u.Id,
                    (x, u) => new
                    {
                        x.UserId,
                        Name = u.FirstName + " " + u.LastName,
                        u.Email,
                        x.OrderCount,
                        x.TotalSpent
                    })
                .OrderByDescending(x => x.TotalSpent)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("CustomerId,Name,Email,OrderCount,TotalSpent");
            var ci = CultureInfo.InvariantCulture;
            foreach (var r in rows)
            {
                sb.Append(r.UserId).Append(',');
                sb.Append(Csv(r.Name)).Append(',');
                sb.Append(Csv(r.Email)).Append(',');
                sb.Append(r.OrderCount).Append(',');
                sb.Append(r.TotalSpent.ToString(ci)).AppendLine();
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                $"customers_{fromUtc:yyyyMMdd}_{toUtc:yyyyMMdd}.csv");
        }

        [HttpGet("admin/export/tickets.csv")]
        [EndpointDescription("Streams a CSV export of support tickets opened or resolved in the given UTC window. Columns: TicketNumber, Subject, Status, Priority, UserId, CreatedAtUtc, ResolvedAtUtc, ResolutionHours. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> ExportTicketsCsv([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (to < from) return BadRequest(new { message = "'to' must be >= 'from'." });

            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            var rows = await _db.SupportTickets
                .Where(t => (t.CreatedAt >= fromUtc && t.CreatedAt <= toUtc) ||
                            (t.ResolvedAt.HasValue && t.ResolvedAt >= fromUtc && t.ResolvedAt <= toUtc))
                .Select(t => new
                {
                    t.TicketNumber,
                    t.Subject,
                    t.Status,
                    t.Priority,
                    t.UserId,
                    t.CreatedAt,
                    t.ResolvedAt
                })
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("TicketNumber,Subject,Status,Priority,UserId,CreatedAtUtc,ResolvedAtUtc,ResolutionHours");
            var ci = CultureInfo.InvariantCulture;
            foreach (var r in rows)
            {
                var hours = r.ResolvedAt.HasValue
                    ? Math.Round((r.ResolvedAt.Value - r.CreatedAt).TotalHours, 2).ToString(ci)
                    : "";

                sb.Append(Csv(r.TicketNumber)).Append(',');
                sb.Append(Csv(r.Subject)).Append(',');
                sb.Append(r.Status).Append(',');
                sb.Append(r.Priority).Append(',');
                sb.Append(r.UserId).Append(',');
                sb.Append(r.CreatedAt.ToString("o", ci)).Append(',');
                sb.Append(r.ResolvedAt?.ToString("o", ci) ?? "").Append(',');
                sb.Append(hours).AppendLine();
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                $"tickets_{fromUtc:yyyyMMdd}_{toUtc:yyyyMMdd}.csv");
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