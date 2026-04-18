using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryDash.Infrastructure.Data.Configurations
{
    public class VendorStaffConfiguration : IEntityTypeConfiguration<VendorStaff>
    {
        public void Configure(EntityTypeBuilder<VendorStaff> builder)
        {
            builder.ToTable("VendorStaff");

            builder.HasKey(vs => vs.Id);

            builder.Property(vs => vs.AssignedDate)
                .IsRequired();

            builder.Property(vs => vs.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            builder.HasOne(vs => vs.User)
                .WithMany()
                .HasForeignKey(vs => vs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(vs => vs.Vendor)
                .WithMany()
                .HasForeignKey(vs => vs.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(vs => vs.UserId)
                .HasDatabaseName("IX_VendorStaff_UserId");

            builder.HasIndex(vs => vs.VendorId)
                .HasDatabaseName("IX_VendorStaff_VendorId");

            builder.HasIndex(vs => new { vs.UserId, vs.VendorId })
                .IsUnique()
                .HasDatabaseName("IX_VendorStaff_UserId_VendorId");

            builder.HasIndex(vs => vs.IsActive)
                .HasDatabaseName("IX_VendorStaff_IsActive");
        }
    }
}