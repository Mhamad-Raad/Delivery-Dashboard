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

            builder.Property(a => a.Type).IsRequired();
            builder.Property(a => a.Latitude).IsRequired();
            builder.Property(a => a.Longitude).IsRequired();

            builder.Property(a => a.PhoneNumber).IsRequired().HasMaxLength(30);
            builder.Property(a => a.Street).IsRequired().HasMaxLength(255);

            builder.Property(a => a.BuildingName).HasMaxLength(255);
            builder.Property(a => a.Floor).HasMaxLength(50);
            builder.Property(a => a.ApartmentNumber).HasMaxLength(50);
            builder.Property(a => a.HouseName).HasMaxLength(255);
            builder.Property(a => a.HouseNumber).HasMaxLength(50);
            builder.Property(a => a.CompanyName).HasMaxLength(255);
            builder.Property(a => a.AdditionalDirections).HasMaxLength(500);
            builder.Property(a => a.Label).HasMaxLength(50);

            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.UserId);

            // Partial unique index — at most one default address per user
            builder.HasIndex(a => a.UserId)
                .HasDatabaseName("IX_Addresses_UserId_IsDefault")
                .HasFilter("\"IsDefault\" = true")
                .IsUnique();
        }
    }
}
