using DeliveryDash.Application.Abstracts.IRepository;
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

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetActiveVendorsCountAsync()
        {
            return await _context.Vendors.CountAsync();
        }

        public Task<int> GetTotalBuildingsCountAsync() => Task.FromResult(0);

        public Task<int> GetTotalApartmentsCountAsync() => Task.FromResult(0);

        public Task<int> GetOccupiedApartmentsCountAsync() => Task.FromResult(0);

        public async Task<int> GetTotalProductsCountAsync()
        {
            return await _context.Products.CountAsync();
        }

        public async Task<int> GetPendingSupportRequestsCountAsync()
        {
            return await _context.SupportTickets
                .Where(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress)
                .CountAsync();
        }

        // Vendor dashboard methods
        public async Task<int> GetVendorTotalOrdersAsync(int vendorId)
        {
            return await _context.Orders
                .Where(o => o.VendorId == vendorId)
                .CountAsync();
        }

        public async Task<int> GetVendorPendingOrdersAsync(int vendorId)
        {
            return await _context.Orders
                .Where(o => o.VendorId == vendorId &&
                       (o.Status == OrderStatus.Pending ||
                        o.Status == OrderStatus.Confirmed ||
                        o.Status == OrderStatus.Preparing))
                .CountAsync();
        }

        public async Task<int> GetVendorCompletedOrdersAsync(int vendorId)
        {
            return await _context.Orders
                .Where(o => o.VendorId == vendorId && o.Status == OrderStatus.Delivered)
                .CountAsync();
        }

        public async Task<int> GetVendorTotalProductsAsync(int vendorId)
        {
            return await _context.Products
                .Where(p => p.VendorId == vendorId)
                .CountAsync();
        }

        public async Task<decimal> GetVendorTotalRevenueAsync(int vendorId)
        {
            return await _context.Orders
                .Where(o => o.VendorId == vendorId && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.Subtotal);
        }

        public async Task<decimal> GetVendorTodayRevenueAsync(int vendorId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Orders
                .Where(o => o.VendorId == vendorId &&
                       o.Status == OrderStatus.Delivered &&
                       o.CreatedAt.Date == today)
                .SumAsync(o => o.Subtotal);
        }

        public async Task<int> GetVendorTodayOrdersAsync(int vendorId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Orders
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
    }
}