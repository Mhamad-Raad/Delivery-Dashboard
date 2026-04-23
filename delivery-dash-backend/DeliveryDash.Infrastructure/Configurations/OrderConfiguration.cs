using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryDash.Infrastructure.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property<uint>("xmin")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.Subtotal)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(o => o.DeliveryFee)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(o => o.TotalAmount)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(o => o.Notes)
                .HasMaxLength(500);

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Vendor)
                .WithMany()
                .HasForeignKey(o => o.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.DeliveryAddress)
                .WithMany()
                .HasForeignKey(o => o.AddressId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(o => o.OrderNumber)
                .IsUnique()
                .HasDatabaseName("IX_Orders_OrderNumber");

            builder.HasIndex(o => o.UserId)
                .HasDatabaseName("IX_Orders_UserId");

            builder.HasIndex(o => o.VendorId)
                .HasDatabaseName("IX_Orders_VendorId");

            builder.HasIndex(o => o.Status)
                .HasDatabaseName("IX_Orders_Status");

            builder.HasIndex(o => o.CreatedAt)
                .HasDatabaseName("IX_Orders_CreatedAt");

            builder.HasIndex(o => o.DeliveredAt)
                .HasDatabaseName("IX_Orders_DeliveredAt");
        }
    }
}