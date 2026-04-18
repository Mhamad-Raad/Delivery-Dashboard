using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryDash.Infrastructure.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.EntityName)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(a => a.EntityId)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(a => a.UserEmail)
                .HasMaxLength(256);

            builder.Property(a => a.OldValues)
                .HasColumnType("jsonb");

            builder.Property(a => a.NewValues)
                .HasColumnType("jsonb");

            builder.Property(a => a.AffectedColumns)
                .HasColumnType("jsonb");

            builder.Property(a => a.UserAgent)
                .HasMaxLength(512);

            // Indexes for efficient querying
            builder.HasIndex(a => a.EntityName)
                .HasDatabaseName("IX_AuditLogs_EntityName");

            builder.HasIndex(a => a.Timestamp)
                .HasDatabaseName("IX_AuditLogs_Timestamp");

            builder.HasIndex(a => a.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");

            builder.HasIndex(a => new { a.EntityName, a.EntityId })
                .HasDatabaseName("IX_AuditLogs_Entity");

            builder.HasIndex(a => a.Action)
                .HasDatabaseName("IX_AuditLogs_Action");

            // Navigation
            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}