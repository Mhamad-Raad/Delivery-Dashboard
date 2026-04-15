using MalDash.Application.Abstracts.IRepository;
using MalDash.Domain.Entities;
using MalDash.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MalDash.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Vendor)
                .Include(o => o.DeliveryAddress)
                    .ThenInclude(a => a!.Building)
                .Include(o => o.DeliveryAddress)
                    .ThenInclude(a => a!.Floor)
                .Include(o => o.DeliveryAddress)
                    .ThenInclude(a => a!.Apartment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Vendor)
                .Include(o => o.DeliveryAddress)
                    .ThenInclude(a => a!.Building)
                .Include(o => o.DeliveryAddress)
                    .ThenInclude(a => a!.Floor)
                .Include(o => o.DeliveryAddress)
                    .ThenInclude(a => a!.Apartment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<(IEnumerable<Order> Orders, int Total)> GetOrdersPagedAsync(
            int page,
            int limit,
            Guid? userId = null,
            int? vendorId = null,
            OrderStatus? status = null,
            IEnumerable<int>? orderIds = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            DateTime? date = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Vendor)
                .Include(o => o.DeliveryAddress)
                    .ThenInclude(a => a!.Building)
                .Include(o => o.DeliveryAddress)
                    .ThenInclude(a => a!.Floor)
                .Include(o => o.DeliveryAddress)
                    .ThenInclude(a => a!.Apartment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(o => o.UserId == userId.Value);
            }

            if (vendorId.HasValue)
            {
                query = query.Where(o => o.VendorId == vendorId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            // Filter by specific order IDs (for driver filtering)
            if (orderIds != null && orderIds.Any())
            {
                query = query.Where(o => orderIds.Contains(o.Id));
            }

            // Date filtering logic
            if (date.HasValue)
            {
                // Filter by specific date (ignores time component)
                var startOfDay = date.Value.Date;
                var endOfDay = startOfDay.AddDays(1);
                query = query.Where(o => o.CreatedAt >= startOfDay && o.CreatedAt < endOfDay);
            }
            else
            {
                // Filter by date range
                if (fromDate.HasValue)
                {
                    var startOfDay = fromDate.Value.Date;
                    query = query.Where(o => o.CreatedAt >= startOfDay);
                }

                if (toDate.HasValue)
                {
                    var endOfDay = toDate.Value.Date.AddDays(1);
                    query = query.Where(o => o.CreatedAt < endOfDay);
                }
            }

            query = query.OrderByDescending(o => o.CreatedAt);

            var total = await query.CountAsync();

            var orders = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (orders, total);
        }

        public async Task<Order> CreateAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            
            // Reload with all navigation properties
            return (await GetByIdAsync(order.Id))!;
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            
            // Reload with all navigation properties
            return (await GetByIdAsync(order.Id))!;
        }

        public async Task<int> GetTodayOrderCountAsync()
        {
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1);

            return await _context.Orders
                .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd)
                .CountAsync();
        }
    }
}