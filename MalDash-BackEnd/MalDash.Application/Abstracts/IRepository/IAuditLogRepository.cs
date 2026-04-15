using MalDash.Domain.Entities;

namespace MalDash.Application.Abstracts.IRepository
{
    public interface IAuditLogRepository
    {
        Task<AuditLog?> GetByIdAsync(long id);
        Task<(IEnumerable<AuditLog> Logs, int Total)> GetPagedAsync(
            int page,
            int limit,
            string? entityName = null,
            string? entityId = null,
            Guid? userId = null,
            string? action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);
        Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityName, string entityId);
        Task CreateRangeAsync(IEnumerable<AuditLog> auditLogs);
    }
}