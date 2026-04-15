using MalDash.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;

namespace MalDash.Infrastructure.Audit
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private List<AuditLog> _pendingAuditLogs = [];
        private List<(EntityEntry Entry, AuditLog Log)> _addedEntryMappings = [];
        private bool _isSavingAuditLogs;

        public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper methods to get user info directly from HttpContext
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private string? GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email)?.Value;
        }

        private string? GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request
                .Headers["User-Agent"].ToString();
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (_isSavingAuditLogs || eventData.Context is null)
                return ValueTask.FromResult(result);

            var context = eventData.Context;

            // Process auditable entity timestamps
            ProcessAuditableEntities(context);

            // Create audit logs and track added entries for ID resolution
            (_pendingAuditLogs, _addedEntryMappings) = CreateAuditLogsWithEntryTracking(context);

            return ValueTask.FromResult(result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (_isSavingAuditLogs || eventData.Context is null || _pendingAuditLogs.Count == 0)
                return result;

            try
            {
                _isSavingAuditLogs = true;

                // Update entity IDs for newly created entities (now they have real IDs)
                foreach (var (entry, log) in _addedEntryMappings)
                {
                    var pk = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                    log.EntityId = pk?.CurrentValue?.ToString() ?? string.Empty;
                }

                // Add audit logs and save
                eventData.Context.Set<AuditLog>().AddRange(_pendingAuditLogs);
                await eventData.Context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                _isSavingAuditLogs = false;
                _pendingAuditLogs = [];
                _addedEntryMappings = [];
            }

            return result;
        }

        private void ProcessAuditableEntities(DbContext context)
        {
            var userId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            foreach (var entry in context.ChangeTracker.Entries<Domain.Common.IAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = userId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedAt = now;
                        entry.Entity.LastModifiedBy = userId;
                        break;
                }
            }
        }

        private (List<AuditLog> Logs, List<(EntityEntry, AuditLog)> AddedMappings) CreateAuditLogsWithEntryTracking(DbContext context)
        {
            var auditLogs = new List<AuditLog>();
            var addedMappings = new List<(EntityEntry, AuditLog)>();

            var userId = GetCurrentUserId();
            var userEmail = GetCurrentUserEmail();
            var userAgent = GetUserAgent();

            context.ChangeTracker.DetectChanges();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (AuditableDbContextExtensions.IsExcludedEntityPublic(entry.Entity.GetType()))
                    continue;

                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditLog = new AuditLog
                {
                    UserId = userId,
                    UserEmail = userEmail,
                    EntityName = entry.Entity.GetType().Name,
                    Timestamp = DateTime.UtcNow,
                    UserAgent = userAgent
                };

                var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditLog.Action = "Created";
                        auditLog.EntityId = string.Empty;
                        auditLog.NewValues = AuditableDbContextExtensions.SerializePropertiesPublic(entry, false);
                        addedMappings.Add((entry, auditLog));
                        break;

                    case EntityState.Deleted:
                        auditLog.Action = "Deleted";
                        auditLog.EntityId = primaryKey?.OriginalValue?.ToString() ?? string.Empty;
                        auditLog.OldValues = AuditableDbContextExtensions.SerializePropertiesPublic(entry, true);
                        break;

                    case EntityState.Modified:
                        auditLog.Action = "Updated";
                        auditLog.EntityId = primaryKey?.CurrentValue?.ToString() ?? string.Empty;
                        var changes = AuditableDbContextExtensions.GetModifiedPropertiesPublic(entry);
                        if (changes.AffectedColumns.Count == 0)
                            continue;
                        auditLog.OldValues = changes.OldValuesJson;
                        auditLog.NewValues = changes.NewValuesJson;
                        auditLog.AffectedColumns = changes.AffectedColumnsJson;
                        break;

                    default:
                        continue;
                }

                auditLogs.Add(auditLog);
            }

            return (auditLogs, addedMappings);
        }
    }
}