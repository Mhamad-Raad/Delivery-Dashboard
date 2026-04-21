using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Domain.Constants;
using DeliveryDash.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------- Home totals ----------

        public Task<int> GetTotalUsersCountAsync() => _context.Users.CountAsync();

        public Task<int> GetActiveVendorsCountAsync() => _context.Vendors.CountAsync();

        public Task<int> GetTotalProductsCountAsync() => _context.Products.CountAsync();

        public Task<int> GetPendingSupportRequestsCountAsync() =>
            _context.SupportTickets
                .Where(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress)
                .CountAsync();

        // ---------- Vendor dashboard ----------

        public Task<int> GetVendorTotalOrdersAsync(int vendorId) =>
            _context.Orders.Where(o => o.VendorId == vendorId).CountAsync();

        public Task<int> GetVendorPendingOrdersAsync(int vendorId) =>
            _context.Orders
                .Where(o => o.VendorId == vendorId &&
                            (o.Status == OrderStatus.Pending ||
                             o.Status == OrderStatus.Confirmed ||
                             o.Status == OrderStatus.Preparing))
                .CountAsync();

        public Task<int> GetVendorCompletedOrdersAsync(int vendorId) =>
            _context.Orders
                .Where(o => o.VendorId == vendorId && o.Status == OrderStatus.Delivered)
                .CountAsync();

        public Task<int> GetVendorTotalProductsAsync(int vendorId) =>
            _context.Products.Where(p => p.VendorId == vendorId).CountAsync();

        public Task<decimal> GetVendorTotalRevenueAsync(int vendorId) =>
            _context.Orders
                .Where(o => o.VendorId == vendorId && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.Subtotal);

        public Task<decimal> GetVendorTodayRevenueAsync(int vendorId)
        {
            var today = DateTime.UtcNow.Date;
            return _context.Orders
                .Where(o => o.VendorId == vendorId &&
                            o.Status == OrderStatus.Delivered &&
                            o.CreatedAt.Date == today)
                .SumAsync(o => o.Subtotal);
        }

        public Task<int> GetVendorTodayOrdersAsync(int vendorId)
        {
            var today = DateTime.UtcNow.Date;
            return _context.Orders
                .Where(o => o.VendorId == vendorId && o.CreatedAt.Date == today)
                .CountAsync();
        }

        public async Task<List<(DateTime Date, int Orders, decimal Revenue)>> GetVendorDailyStatsAsync(int vendorId, int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

            var stats = await _context.Orders
                .Where(o => o.VendorId == vendorId && o.CreatedAt.Date >= startDate)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Orders = g.Count(),
                    Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Subtotal)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return stats.Select(s => (s.Date, s.Orders, s.Revenue)).ToList();
        }

        // ---------- Admin analytics ----------

        public async Task<List<(DateTime Bucket, int Orders, decimal Revenue)>> GetOrderTimeSeriesAsync(DateTime fromUtc, DateTime toUtc, string granularity)
        {
            var daily = await _context.Orders
                .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Orders = g.Count(),
                    Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Subtotal)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return RollUpTimeSeries(daily.Select(d => (d.Date, d.Orders, d.Revenue)).ToList(), granularity);
        }

        public async Task<(int Total, int Delivered, int Cancelled, decimal Revenue, decimal Gmv)> GetOrderTotalsAsync(DateTime fromUtc, DateTime toUtc)
        {
            var rows = await _context.Orders
                .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc)
                .GroupBy(o => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Delivered = g.Count(o => o.Status == OrderStatus.Delivered),
                    Cancelled = g.Count(o => o.Status == OrderStatus.Cancelled),
                    Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Subtotal),
                    Gmv = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount)
                })
                .FirstOrDefaultAsync();

            return rows == null
                ? (0, 0, 0, 0m, 0m)
                : (rows.Total, rows.Delivered, rows.Cancelled, rows.Revenue, rows.Gmv);
        }

        public async Task<List<(OrderStatus Status, int Count)>> GetOrderStatusBreakdownAsync(DateTime fromUtc, DateTime toUtc)
        {
            var rows = await _context.Orders
                .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc)
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return rows.Select(r => (r.Status, r.Count)).ToList();
        }

        public async Task<List<(string CategoryName, decimal Revenue)>> GetRevenueByVendorCategoryAsync(DateTime fromUtc, DateTime toUtc)
        {
            var rows = await _context.Orders
                .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc && o.Status == OrderStatus.Delivered)
                .Join(_context.Vendors, o => o.VendorId, v => v.Id, (o, v) => new { o.Subtotal, v.VendorCategoryId })
                .Join(_context.VendorCategories, ov => ov.VendorCategoryId, vc => vc.Id,
                      (ov, vc) => new { vc.Name, ov.Subtotal })
                .GroupBy(x => x.Name)
                .Select(g => new { Name = g.Key, Revenue = g.Sum(x => x.Subtotal) })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            return rows.Select(r => (r.Name, r.Revenue)).ToList();
        }

        public async Task<List<(int VendorId, string Name, string CategoryName, int OrderCount, decimal Revenue)>> GetTopVendorsByRevenueAsync(DateTime fromUtc, DateTime toUtc, int take)
        {
            var rows = await _context.Orders
                .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc && o.Status == OrderStatus.Delivered)
                .GroupBy(o => o.VendorId)
                .Select(g => new { VendorId = g.Key, OrderCount = g.Count(), Revenue = g.Sum(o => o.Subtotal) })
                .OrderByDescending(x => x.Revenue)
                .Take(take)
                .Join(_context.Vendors, x => x.VendorId, v => v.Id,
                      (x, v) => new { x.VendorId, v.Name, v.VendorCategoryId, x.OrderCount, x.Revenue })
                .Join(_context.VendorCategories, x => x.VendorCategoryId, vc => vc.Id,
                      (x, vc) => new { x.VendorId, x.Name, CategoryName = vc.Name, x.OrderCount, x.Revenue })
                .ToListAsync();

            return rows.Select(r => (r.VendorId, r.Name, r.CategoryName, r.OrderCount, r.Revenue)).ToList();
        }

        public async Task<List<(int VendorId, string Name, string CategoryName, int OrderCount, decimal Revenue)>> GetTopVendorsByOrdersAsync(DateTime fromUtc, DateTime toUtc, int take)
        {
            var rows = await _context.Orders
                .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc)
                .GroupBy(o => o.VendorId)
                .Select(g => new
                {
                    VendorId = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Subtotal)
                })
                .OrderByDescending(x => x.OrderCount)
                .Take(take)
                .Join(_context.Vendors, x => x.VendorId, v => v.Id,
                      (x, v) => new { x.VendorId, v.Name, v.VendorCategoryId, x.OrderCount, x.Revenue })
                .Join(_context.VendorCategories, x => x.VendorCategoryId, vc => vc.Id,
                      (x, vc) => new { x.VendorId, x.Name, CategoryName = vc.Name, x.OrderCount, x.Revenue })
                .ToListAsync();

            return rows.Select(r => (r.VendorId, r.Name, r.CategoryName, r.OrderCount, r.Revenue)).ToList();
        }

        public Task<int> GetVendorSignupsCountAsync(DateTime fromUtc, DateTime toUtc) =>
            _context.Vendors.CountAsync(v => v.CreatedAt >= fromUtc && v.CreatedAt <= toUtc);

        public async Task<(int Active, int Inactive)> GetVendorActivityCountsAsync(DateTime fromUtc, DateTime toUtc)
        {
            var totalVendors = await _context.Vendors.CountAsync();
            var activeVendors = await _context.Orders
                .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc)
                .Select(o => o.VendorId)
                .Distinct()
                .CountAsync();

            return (activeVendors, totalVendors - activeVendors);
        }

        public async Task<List<(Guid DriverId, string Name, int Deliveries, double AvgMinutes)>> GetTopDriversByDeliveriesAsync(DateTime fromUtc, DateTime toUtc, int take)
        {
            var raw = await _context.OrderAssignments
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

            return raw
                .GroupBy(r => new { r.DriverId, r.Name })
                .Select(g => (
                    DriverId: g.Key.DriverId,
                    Name: g.Key.Name,
                    Deliveries: g.Count(),
                    AvgMinutes: g.Average(r => (r.CompletedAt - r.RespondedAt).TotalMinutes)))
                .OrderByDescending(x => x.Deliveries)
                .Take(take)
                .ToList();
        }

        public async Task<double> GetAvgDeliveryMinutesAsync(DateTime fromUtc, DateTime toUtc)
        {
            var raw = await _context.OrderAssignments
                .Where(a => a.Status == OrderAssignmentStatus.Accepted &&
                            a.Order.Status == OrderStatus.Delivered &&
                            a.Order.CompletedAt.HasValue &&
                            a.Order.CompletedAt >= fromUtc &&
                            a.Order.CompletedAt <= toUtc &&
                            a.RespondedAt.HasValue)
                .Select(a => new
                {
                    RespondedAt = a.RespondedAt!.Value,
                    CompletedAt = a.Order.CompletedAt!.Value
                })
                .ToListAsync();

            return raw.Count == 0
                ? 0
                : raw.Average(r => (r.CompletedAt - r.RespondedAt).TotalMinutes);
        }

        public async Task<int> GetActiveDriversCountAsync(DateTime fromUtc, DateTime toUtc)
        {
            return await _context.DriverShifts
                .Where(s => s.StartedAt <= toUtc && (s.EndedAt == null || s.EndedAt >= fromUtc))
                .Select(s => s.DriverId)
                .Distinct()
                .CountAsync();
        }

        public Task<int> GetCustomerSignupsCountAsync(DateTime fromUtc, DateTime toUtc)
        {
            var customerRoleId = IdentityRoleConstant.CustomerRoleGuid;
            return (from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    where ur.RoleId == customerRoleId &&
                          u.CreatedAt >= fromUtc && u.CreatedAt <= toUtc
                    select u.Id).CountAsync();
        }

        public async Task<List<(DateTime Bucket, int Count)>> GetCustomerSignupSeriesAsync(DateTime fromUtc, DateTime toUtc, string granularity)
        {
            var customerRoleId = IdentityRoleConstant.CustomerRoleGuid;

            var daily = await (from u in _context.Users
                               join ur in _context.UserRoles on u.Id equals ur.UserId
                               where ur.RoleId == customerRoleId &&
                                     u.CreatedAt >= fromUtc && u.CreatedAt <= toUtc
                               group u by u.CreatedAt.Date into g
                               orderby g.Key
                               select new { Date = g.Key, Count = g.Count() })
                              .ToListAsync();

            return RollUpCountSeries(daily.Select(d => (d.Date, d.Count)).ToList(), granularity);
        }

        public async Task<List<(Guid CustomerId, string Name, int OrderCount, decimal TotalSpent)>> GetTopSpendersAsync(DateTime fromUtc, DateTime toUtc, int take)
        {
            var rows = await _context.Orders
                .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc && o.Status == OrderStatus.Delivered)
                .GroupBy(o => o.UserId)
                .Select(g => new { UserId = g.Key, OrderCount = g.Count(), TotalSpent = g.Sum(o => o.TotalAmount) })
                .OrderByDescending(x => x.TotalSpent)
                .Take(take)
                .Join(_context.Users, x => x.UserId, u => u.Id,
                      (x, u) => new
                      {
                          x.UserId,
                          Name = u.FirstName + " " + u.LastName,
                          x.OrderCount,
                          x.TotalSpent
                      })
                .ToListAsync();

            return rows.Select(r => (r.UserId, r.Name, r.OrderCount, r.TotalSpent)).ToList();
        }

        public async Task<(int Returning, int OneTime)> GetReturningVsOneTimeCustomersAsync(DateTime fromUtc, DateTime toUtc)
        {
            var customerOrderCounts = await _context.Orders
                .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc)
                .GroupBy(o => o.UserId)
                .Select(g => g.Count())
                .ToListAsync();

            var returning = customerOrderCounts.Count(c => c > 1);
            var oneTime = customerOrderCounts.Count(c => c == 1);

            return (returning, oneTime);
        }

        public async Task<(int Opened, int Resolved, int OpenBacklog, double AvgResolutionHours)> GetSupportKpisAsync(DateTime fromUtc, DateTime toUtc)
        {
            var opened = await _context.SupportTickets
                .CountAsync(t => t.CreatedAt >= fromUtc && t.CreatedAt <= toUtc);

            var resolved = await _context.SupportTickets
                .CountAsync(t => t.ResolvedAt.HasValue &&
                                 t.ResolvedAt >= fromUtc && t.ResolvedAt <= toUtc);

            var openBacklog = await _context.SupportTickets
                .CountAsync(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress);

            var resolvedTickets = await _context.SupportTickets
                .Where(t => t.ResolvedAt.HasValue &&
                            t.ResolvedAt >= fromUtc && t.ResolvedAt <= toUtc)
                .Select(t => new { t.CreatedAt, ResolvedAt = t.ResolvedAt!.Value })
                .ToListAsync();

            var avgHours = resolvedTickets.Count == 0
                ? 0
                : resolvedTickets.Average(t => (t.ResolvedAt - t.CreatedAt).TotalHours);

            return (opened, resolved, openBacklog, avgHours);
        }

        private static List<(DateTime Bucket, int Orders, decimal Revenue)> RollUpTimeSeries(
            List<(DateTime Date, int Orders, decimal Revenue)> daily, string granularity)
        {
            return granularity?.ToLowerInvariant() switch
            {
                "week" => daily
                    .GroupBy(d => StartOfWeek(d.Date))
                    .Select(g => (Bucket: g.Key, Orders: g.Sum(x => x.Orders), Revenue: g.Sum(x => x.Revenue)))
                    .OrderBy(x => x.Bucket)
                    .ToList(),
                "month" => daily
                    .GroupBy(d => new DateTime(d.Date.Year, d.Date.Month, 1))
                    .Select(g => (Bucket: g.Key, Orders: g.Sum(x => x.Orders), Revenue: g.Sum(x => x.Revenue)))
                    .OrderBy(x => x.Bucket)
                    .ToList(),
                _ => daily.Select(d => (Bucket: d.Date, d.Orders, d.Revenue)).ToList()
            };
        }

        private static List<(DateTime Bucket, int Count)> RollUpCountSeries(
            List<(DateTime Date, int Count)> daily, string granularity)
        {
            return granularity?.ToLowerInvariant() switch
            {
                "week" => daily
                    .GroupBy(d => StartOfWeek(d.Date))
                    .Select(g => (Bucket: g.Key, Count: g.Sum(x => x.Count)))
                    .OrderBy(x => x.Bucket)
                    .ToList(),
                "month" => daily
                    .GroupBy(d => new DateTime(d.Date.Year, d.Date.Month, 1))
                    .Select(g => (Bucket: g.Key, Count: g.Sum(x => x.Count)))
                    .OrderBy(x => x.Bucket)
                    .ToList(),
                _ => daily.Select(d => (Bucket: d.Date, d.Count)).ToList()
            };
        }

        private static DateTime StartOfWeek(DateTime d)
        {
            var diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
            return d.Date.AddDays(-diff);
        }
    }
}
