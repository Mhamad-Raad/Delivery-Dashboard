using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Infrastructure.Repositories
{
    public class DriverShiftRepository : IDriverShiftRepository
    {
        private readonly ApplicationDbContext _context;

        public DriverShiftRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DriverShift?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.DriverShifts
                .Include(s => s.Driver)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<DriverShift?> GetActiveShiftAsync(Guid driverId, CancellationToken ct = default)
        {
            return await _context.DriverShifts
                .Include(s => s.Driver)
                .FirstOrDefaultAsync(s => s.DriverId == driverId && s.EndedAt == null, ct);
        }

        public async Task<IEnumerable<DriverShift>> GetShiftsByDriverIdAsync(
            Guid driverId, int page = 1, int limit = 10, CancellationToken ct = default)
        {
            return await _context.DriverShifts
                .Include(s => s.Driver)
                .Where(s => s.DriverId == driverId)
                .OrderByDescending(s => s.StartedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync(ct);
        }

        public async Task<DriverShift> CreateAsync(DriverShift shift, CancellationToken ct = default)
        {
            await _context.DriverShifts.AddAsync(shift, ct);
            await _context.SaveChangesAsync(ct);
            return shift;
        }

        public async Task<DriverShift> UpdateAsync(DriverShift shift, CancellationToken ct = default)
        {
            _context.DriverShifts.Update(shift);
            await _context.SaveChangesAsync(ct);
            return shift;
        }
    }
}