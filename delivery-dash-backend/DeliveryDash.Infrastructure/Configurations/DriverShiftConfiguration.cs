using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryDash.Infrastructure.Configurations
{
    public class DriverShiftConfiguration : IEntityTypeConfiguration<DriverShift>
    {
        public void Configure(EntityTypeBuilder<DriverShift> builder)
        {
            builder.ToTable("DriverShifts");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.StartedAt)
                .IsRequired();

            builder.HasIndex(s => s.DriverId)
                .HasDatabaseName("IX_DriverShifts_DriverId");

            builder.HasIndex(s => new { s.DriverId, s.EndedAt })
                .HasDatabaseName("IX_DriverShifts_DriverId_EndedAt")
                .HasFilter("\"EndedAt\" IS NULL");

            builder.HasOne(s => s.Driver)
                .WithMany()
                .HasForeignKey(s => s.DriverId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}