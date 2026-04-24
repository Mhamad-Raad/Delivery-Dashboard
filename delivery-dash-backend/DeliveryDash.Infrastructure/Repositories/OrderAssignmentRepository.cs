using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Infrastructure.Repositories
{
    public class OrderAssignmentRepository : IOrderAssignmentRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderAssignmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderAssignment?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.OrderAssignments
                .Include(a => a.Order)
                .Include(a => a.Driver)
                .Include(a => a.Shift)
                .FirstOrDefaultAsync(a => a.Id == id, ct);
        }

        public async Task<OrderAssignment?> GetPendingAssignmentForOrderAsync(int orderId, CancellationToken ct = default)
        {
            return await _context.OrderAssignments
                .Include(a => a.Driver)
                .FirstOrDefaultAsync(a => 
                    a.OrderId == orderId && 
                    a.Status == OrderAssignmentStatus.Pending, ct);
        }

        public async Task<OrderAssignment?> GetAcceptedAssignmentAsync(int orderId, Guid driverId, CancellationToken ct = default)
        {
            return await _context.OrderAssignments
                .Include(a => a.Order)
                .FirstOrDefaultAsync(a =>
                    a.OrderId == orderId &&
                    a.DriverId == driverId &&
                    a.Status == OrderAssignmentStatus.Accepted, ct);
        }

        public async Task<OrderAssignment?> GetAcceptedAssignmentByOrderAsync(int orderId, CancellationToken ct = default)
        {
            return await _context.OrderAssignments
                .Include(a => a.Driver)
                .FirstOrDefaultAsync(a =>
                    a.OrderId == orderId &&
                    a.Status == OrderAssignmentStatus.Accepted, ct);
        }

        public async Task<IEnumerable<OrderAssignment>> GetExpiredAssignmentsAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await _context.OrderAssignments
                .Include(a => a.Order)
                .Include(a => a.Shift)
                .Where(a => 
                    a.Status == OrderAssignmentStatus.Pending && 
                    a.ExpiresAt < now)
                .ToListAsync(ct);
        }

        public async Task<OrderAssignment> CreateAsync(OrderAssignment assignment, CancellationToken ct = default)
        {
            await _context.OrderAssignments.AddAsync(assignment, ct);
            await _context.SaveChangesAsync(ct);
            return assignment;
        }

        public async Task<OrderAssignment> UpdateAsync(OrderAssignment assignment, CancellationToken ct = default)
        {
            _context.OrderAssignments.Update(assignment);
            await _context.SaveChangesAsync(ct);
            return assignment;
        }

        public async Task<bool> HasAssignmentForDriverAsync(int orderId, Guid driverId, CancellationToken ct = default)
        {
            return await _context.OrderAssignments
                .AnyAsync(a => a.OrderId == orderId && a.DriverId == driverId, ct);
        }

        public async Task<IEnumerable<int>> GetOrderIdsForDriverAsync(Guid driverId, CancellationToken ct = default)
        {
            return await _context.OrderAssignments
                .Where(a => a.DriverId == driverId)
                .Select(a => a.OrderId)
                .Distinct()
                .ToListAsync(ct);
        }

        public async Task<int> GetAssignmentCountForOrderAsync(int orderId, CancellationToken ct = default)
        {
            return await _context.OrderAssignments
                .CountAsync(a => a.OrderId == orderId, ct);
        }
    }
}