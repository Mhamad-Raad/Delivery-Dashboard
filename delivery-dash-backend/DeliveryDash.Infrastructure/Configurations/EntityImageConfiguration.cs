using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryDash.Infrastructure.Configurations
{
    public class EntityImageConfiguration : IEntityTypeConfiguration<EntityImage>
    {
        public void Configure(EntityTypeBuilder<EntityImage> builder)
        {
            builder.ToTable("EntityImages");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            builder.Property(e => e.EntityId).IsRequired().HasMaxLength(128);
            builder.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);

            builder.HasIndex(e => new { e.EntityType, e.EntityId });
        }
    }
}