using DeliveryDash.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryDash.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(e => e.FirstName)
                .HasMaxLength(25)
                .IsRequired();

            builder.Property(e => e.LastName)
                .HasMaxLength(25)
                .IsRequired();

            builder.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            builder.HasIndex(u => u.CreatedAt)
                .HasDatabaseName("IX_Users_CreatedAt");

            // B-tree indexes
            builder.HasIndex(u => u.Email)
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(u => u.FirstName)
                .HasDatabaseName("IX_Users_FirstName");

            builder.HasIndex(u => u.LastName)
                .HasDatabaseName("IX_Users_LastName");

            builder.HasIndex(u => u.PhoneNumber)
                .HasDatabaseName("IX_Users_PhoneNumber")
                .HasFilter("\"PhoneNumber\" IS NOT NULL");

            builder.HasIndex(u => u.RefreshToken)
                .HasDatabaseName("IX_Users_RefreshToken")
                .HasFilter("\"RefreshToken\" IS NOT NULL");

            // GIN Trigram Indexes
            builder.HasIndex(u => u.Email)
                .HasDatabaseName("IX_Users_Email_Trgm")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            builder.HasIndex(u => u.FirstName)
                .HasDatabaseName("IX_Users_FirstName_Trgm")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            builder.HasIndex(u => u.LastName)
                .HasDatabaseName("IX_Users_LastName_Trgm")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            builder.HasIndex(u => u.PhoneNumber)
                .HasDatabaseName("IX_Users_PhoneNumber_Trgm")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops")
                .HasFilter("\"PhoneNumber\" IS NOT NULL");

            // Composite index
            builder.HasIndex(u => new { u.Email, u.FirstName, u.LastName })
                .HasDatabaseName("IX_Users_Search_Composite");
        }
    }
}