using MalDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MalDash.Infrastructure.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Name)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(p => p.Price)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(p => p.DiscountPrice)
                .HasColumnType("decimal(10,2)");

            builder.Property(p => p.ProductImageUrl)
                .IsRequired();

            builder.HasOne(p => p.Vendor)
                .WithMany()
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(p => p.VendorId)
                .HasDatabaseName("IX_Products_VendorId");

            builder.HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_Products_CategoryId");

            builder.HasIndex(p => p.InStock)
                .HasDatabaseName("IX_Products_InStock");

            // Regular B-tree index for Name (no unique needed for products)
            builder.HasIndex(p => p.Name)
                .HasDatabaseName("IX_Products_Name");

            // GIN trigram index for Product name searches
            builder.HasIndex(p => p.Name)
                .HasDatabaseName("IX_Products_Name_Trgm")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            builder.HasIndex(p => new { p.VendorId, p.InStock })
                .HasDatabaseName("IX_Products_VendorId_InStock");
        }
    }
}