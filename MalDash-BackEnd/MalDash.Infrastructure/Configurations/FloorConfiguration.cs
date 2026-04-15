using MalDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MalDash.Infrastructure.Configurations
{
    public class FloorConfiguration : IEntityTypeConfiguration<Floor>
    {
        public void Configure(EntityTypeBuilder<Floor> builder)
        {
            builder.HasKey(f => f.Id);
            
            builder.Property(f => f.FloorNumber).IsRequired();

            builder.HasMany(f => f.Apartments)
                .WithOne(a => a.Floor)
                .HasForeignKey(a => a.FloorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(f => new { f.BuildingId, f.FloorNumber }).IsUnique();
        }
    }
}