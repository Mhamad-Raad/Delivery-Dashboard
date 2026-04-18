using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryDash.Infrastructure.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.HasKey(a => a.Id);

            // Relationships
            builder.HasOne(a => a.Building)
                .WithMany(b => b.Addresses)
                .HasForeignKey(a => a.BuildingId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.Floor)
                .WithMany(f => f.Addresses)
                .HasForeignKey(a => a.FloorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.Apartment)
                .WithMany(ap => ap.Addresses)
                .HasForeignKey(a => a.ApartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(a => a.BuildingId);
            builder.HasIndex(a => a.FloorId);
            builder.HasIndex(a => a.ApartmentId);
            builder.HasIndex(a => a.UserId).IsUnique();
            
            builder.HasIndex(a => new { a.BuildingId, a.FloorId, a.ApartmentId });

            builder.HasIndex(a => new { a.UserId, a.BuildingId })
                .HasDatabaseName("IX_Addresses_UserId_BuildingId")
                .HasFilter("\"UserId\" IS NOT NULL AND \"BuildingId\" IS NOT NULL");

            builder.HasIndex(a => a.UserId)
                .HasDatabaseName("IX_Addresses_UserId")
                .HasFilter("\"UserId\" IS NOT NULL");

            builder.HasIndex(a => a.BuildingId)
                .HasDatabaseName("IX_Addresses_BuildingId")
                .HasFilter("\"BuildingId\" IS NOT NULL");
        }
    }
}