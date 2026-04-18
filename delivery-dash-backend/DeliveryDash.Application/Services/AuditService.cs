using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Responses.AuditResponses;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Domain.Entities;
using System.Text.Json;

namespace DeliveryDash.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<PagedResponse<AuditLogResponse>> GetAuditLogsAsync(
            int page = 1,
            int limit = 20,
            string? entityName = null,
            string? entityId = null,
            Guid? userId = null,
            string? action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var (logs, total) = await _auditLogRepository.GetPagedAsync(
                page, limit, entityName, entityId, userId, action, fromDate, toDate);

            var responses = logs.Select(MapToResponse).ToList();

            return new PagedResponse<AuditLogResponse>
            {
                Data = responses,
                Page = page,
                Limit = limit,
                Total = total
            };
        }

        public async Task<AuditLogResponse?> GetByIdAsync(long id)
        {
            var log = await _auditLogRepository.GetByIdAsync(id);
            return log == null ? null : MapToResponse(log);
        }

        public async Task<IEnumerable<AuditLogResponse>> GetEntityHistoryAsync(string entityName, string entityId)
        {
            var logs = await _auditLogRepository.GetEntityHistoryAsync(entityName, entityId);
            return logs.Select(MapToResponse);
        }

        private static AuditLogResponse MapToResponse(AuditLog log)
        {
            return new AuditLogResponse
            {
                Id = log.Id,
                UserId = log.UserId,
                UserEmail = log.UserEmail,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                Action = log.Action,
                OldValues = string.IsNullOrEmpty(log.OldValues) 
                    ? null 
                    : JsonSerializer.Deserialize<Dictionary<string, object?>>(log.OldValues),
                NewValues = string.IsNullOrEmpty(log.NewValues) 
                    ? null 
                    : JsonSerializer.Deserialize<Dictionary<string, object?>>(log.NewValues),
                AffectedColumns = string.IsNullOrEmpty(log.AffectedColumns) 
                    ? null 
                    : JsonSerializer.Deserialize<List<string>>(log.AffectedColumns),
                Timestamp = log.Timestamp,
                ProfileImageUrl = log.User?.ProfileImageUrl
            };
        }
    }
}