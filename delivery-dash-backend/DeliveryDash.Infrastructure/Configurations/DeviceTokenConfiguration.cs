using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryDash.Infrastructure.Configurations
{
    public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
    {
        public void Configure(EntityTypeBuilder<DeviceToken> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(d => d.Platform)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(d => d.CreatedAt)
                .IsRequired();

            builder.Property(d => d.LastSeenAt)
                .IsRequired();

            builder.HasIndex(d => d.Token)
                .IsUnique()
                .HasDatabaseName("IX_DeviceTokens_Token");

            builder.HasIndex(d => d.UserId)
                .HasDatabaseName("IX_DeviceTokens_UserId");

            builder.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
