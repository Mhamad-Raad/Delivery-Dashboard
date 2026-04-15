using MalDash.Application.Abstracts.IRepository;
using MalDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MalDash.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuditLog?> GetByIdAsync(long id)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<(IEnumerable<AuditLog> Logs, int Total)> GetPagedAsync(
            int page,
            int limit,
            string? entityName = null,
            string? entityId = null,
            Guid? userId = null,
            string? action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityName))
                query = query.Where(a => a.EntityName == entityName);

            if (!string.IsNullOrWhiteSpace(entityId))
                query = query.Where(a => a.EntityId == entityId);

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(a => a.Action == action);

            if (fromDate.HasValue)
                query = query.Where(a => a.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Timestamp <= toDate.Value);

            var total = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (logs, total);
        }

        public async Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityName, string entityId)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .AsNoTracking()
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task CreateRangeAsync(IEnumerable<AuditLog> auditLogs)
        {
            await _context.AuditLogs.AddRangeAsync(auditLogs);
            await _context.SaveChangesAsync();
        }
    }
}