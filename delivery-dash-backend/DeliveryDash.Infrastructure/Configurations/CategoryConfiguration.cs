using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryDash.Infrastructure.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.HasOne(c => c.Vendor)
                .WithMany()
                .HasForeignKey(c => c.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => new { c.VendorId, c.Name })
                .IsUnique()
                .HasDatabaseName("IX_Categories_VendorId_Name");

            builder.HasIndex(c => c.VendorId)
                .HasDatabaseName("IX_Categories_VendorId");

            builder.HasIndex(c => c.Name)
                .HasDatabaseName("IX_Categories_Name_Trgm")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");
        }
    }
}
