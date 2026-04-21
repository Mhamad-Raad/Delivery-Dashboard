using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryDash.Infrastructure.Configurations
{
    public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
    {
        public void Configure(EntityTypeBuilder<Vendor> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(v => v.Description)
                .HasMaxLength(500);

            builder.Property(v => v.OpeningTime)
                .IsRequired();

            builder.Property(v => v.CloseTime)
                .IsRequired();

            builder.Property(v => v.VendorCategoryId)
                .IsRequired();

            builder.Property(v => v.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            builder.HasIndex(v => v.CreatedAt)
                .HasDatabaseName("IX_Vendors_CreatedAt");

            builder.HasOne(v => v.VendorCategory)
                .WithMany(vc => vc.Vendors)
                .HasForeignKey(v => v.VendorCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(v => v.Name)
                .IsUnique()
                .HasDatabaseName("IX_Vendors_Name");

            builder.HasIndex(v => v.UserId)
                .IsUnique()
                .HasDatabaseName("IX_Vendors_UserId");

            builder.HasIndex(v => v.VendorCategoryId)
                .HasDatabaseName("IX_Vendors_VendorCategoryId");

            builder.HasIndex(v => v.Name)
                .HasDatabaseName("IX_Vendors_Name_Trgm")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");
        }
    }
}
