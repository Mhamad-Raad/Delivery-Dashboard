using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryDash.Infrastructure.Configurations
{
    public class VendorCategoryConfiguration : IEntityTypeConfiguration<VendorCategory>
    {
        public void Configure(EntityTypeBuilder<VendorCategory> builder)
        {
            builder.HasKey(vc => vc.Id);

            builder.Property(vc => vc.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(vc => vc.Description)
                .HasMaxLength(500);

            builder.Property(vc => vc.IsActive)
                .IsRequired();

            builder.Property(vc => vc.CreatedAt)
                .IsRequired();

            builder.Property(vc => vc.UpdatedAt)
                .IsRequired();

            builder.HasIndex(vc => vc.Name)
                .IsUnique()
                .HasDatabaseName("IX_VendorCategories_Name");

            builder.HasIndex(vc => vc.Name)
                .HasDatabaseName("IX_VendorCategories_Name_Trgm")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            builder.HasIndex(vc => vc.IsActive)
                .HasDatabaseName("IX_VendorCategories_IsActive");
        }
    }
}
