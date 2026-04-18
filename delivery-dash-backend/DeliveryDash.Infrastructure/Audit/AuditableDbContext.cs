using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Domain.Common;
using DeliveryDash.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace DeliveryDash.Infrastructure.Audit
{
    public static class AuditableDbContextExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Entities that should NOT be audited
        private static readonly HashSet<Type> ExcludedEntityTypes =
        [
            typeof(AuditLog),
            typeof(Notification),
            typeof(IdentityUserRole<Guid>),
            typeof(IdentityUserClaim<Guid>),
            typeof(IdentityUserLogin<Guid>),
            typeof(IdentityUserToken<Guid>),
            typeof(IdentityRoleClaim<Guid>),
            typeof(IdentityRole<Guid>)
        ];

        private static readonly HashSet<string> SkipProperties = new(StringComparer.OrdinalIgnoreCase)
        {
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "RefreshToken",
            "RefreshTokenExpiresAtUtc"
        };

        public static void ProcessAuditableEntities(
            this DbContext context,
            ICurrentUserService? currentUserService)
        {
            var userId = currentUserService?.GetCurrentUserIdOrNull();
            var now = DateTime.UtcNow;

            foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
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

        public static bool IsExcludedEntityPublic(Type entityType)
        {
            if (ExcludedEntityTypes.Contains(entityType))
                return true;

            if (entityType.IsGenericType)
            {
                var genericDef = entityType.GetGenericTypeDefinition();
                if (genericDef == typeof(IdentityUserRole<>) ||
                    genericDef == typeof(IdentityUserClaim<>) ||
                    genericDef == typeof(IdentityUserLogin<>) ||
                    genericDef == typeof(IdentityUserToken<>) ||
                    genericDef == typeof(IdentityRoleClaim<>) ||
                    genericDef == typeof(IdentityRole<>))
                {
                    return true;
                }
            }

            return false;
        }

        public static string SerializePropertiesPublic(EntityEntry entry, bool useOriginal)
        {
            var properties = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;

                if (SkipProperties.Contains(property.Metadata.Name))
                    continue;

                var value = useOriginal ? property.OriginalValue : property.CurrentValue;
                properties[property.Metadata.Name] = value;
            }

            return JsonSerializer.Serialize(properties, JsonOptions);
        }

        public static (string OldValuesJson, string NewValuesJson, string AffectedColumnsJson, List<string> AffectedColumns)
            GetModifiedPropertiesPublic(EntityEntry entry)
        {
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var affectedColumns = new List<string>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;

                if (SkipProperties.Contains(property.Metadata.Name))
                    continue;

                if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                {
                    oldValues[property.Metadata.Name] = property.OriginalValue;
                    newValues[property.Metadata.Name] = property.CurrentValue;
                    affectedColumns.Add(property.Metadata.Name);
                }
            }

            return (
                JsonSerializer.Serialize(oldValues, JsonOptions),
                JsonSerializer.Serialize(newValues, JsonOptions),
                JsonSerializer.Serialize(affectedColumns, JsonOptions),
                affectedColumns
            );
        }
    }
}