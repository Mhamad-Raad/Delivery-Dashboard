using MalDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MalDash.Infrastructure.Configurations
{
    public class BuildingConfiguration : IEntityTypeConfiguration<Building>
    {
        public void Configure(EntityTypeBuilder<Building> builder)
        {
            builder.HasKey(b => b.Id);
            
            builder.Property(b => b.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(b => b.Name).IsUnique();

            builder.HasMany(b => b.Floors)
                .WithOne(f => f.Building)
                .HasForeignKey(f => f.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(b => b.Name)
                .HasDatabaseName("IX_Buildings_Name");

            builder.HasIndex(b => b.Name)
                .HasDatabaseName("IX_Buildings_Name_Trgm")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");
        }
    }
}