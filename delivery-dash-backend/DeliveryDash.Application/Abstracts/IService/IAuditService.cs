using DeliveryDash.Application.Responses.AuditResponses;
using DeliveryDash.Application.Responses.Common;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IAuditService
    {
        Task<PagedResponse<AuditLogResponse>> GetAuditLogsAsync(
            int page = 1,
            int limit = 20,
            string? entityName = null,
            string? entityId = null,
            Guid? userId = null,
            string? action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        Task<AuditLogResponse?> GetByIdAsync(long id);
        Task<IEnumerable<AuditLogResponse>> GetEntityHistoryAsync(string entityName, string entityId);
    }
}