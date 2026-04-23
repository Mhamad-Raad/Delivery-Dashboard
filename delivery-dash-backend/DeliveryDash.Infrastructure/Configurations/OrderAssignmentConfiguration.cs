using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryDash.Infrastructure.Configurations
{
    public class OrderAssignmentConfiguration : IEntityTypeConfiguration<OrderAssignment>
    {
        public void Configure(EntityTypeBuilder<OrderAssignment> builder)
        {
            builder.ToTable("OrderAssignments");

            builder.HasKey(a => a.Id);

            builder.Property<uint>("xmin")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            builder.Property(a => a.OfferedAt)
                .IsRequired();

            builder.Property(a => a.ExpiresAt)
                .IsRequired();

            builder.Property(a => a.Status)
                .IsRequired();

            builder.HasIndex(a => a.OrderId)
                .HasDatabaseName("IX_OrderAssignments_OrderId");

            builder.HasIndex(a => a.DriverId)
                .HasDatabaseName("IX_OrderAssignments_DriverId");

            builder.HasIndex(a => new { a.OrderId, a.Status })
                .HasDatabaseName("IX_OrderAssignments_OrderId_Status");

            builder.HasIndex(a => new { a.Status, a.ExpiresAt })
                .HasDatabaseName("IX_OrderAssignments_Status_ExpiresAt")
                .HasFilter("\"Status\" = 0"); // Pending status

            builder.HasIndex(a => a.OrderId)
                .IsUnique()
                .HasFilter("\"Status\" = 1") // Accepted — enforce one accepted assignment per order
                .HasDatabaseName("UX_OrderAssignments_OrderId_Accepted");

            builder.HasOne(a => a.Order)
                .WithMany()
                .HasForeignKey(a => a.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Driver)
                .WithMany()
                .HasForeignKey(a => a.DriverId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Shift)
                .WithMany(s => s.OrderAssignments)
                .HasForeignKey(a => a.ShiftId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}