using MalDash.Domain.Entities;
using MalDash.Domain.Entities.FloorPlan;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace MalDash.Infrastructure.Configurations
{
    public class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public void Configure(EntityTypeBuilder<Apartment> builder)
        {
            builder.HasKey(a => a.Id);
            
            builder.Property(a => a.ApartmentName).IsRequired();

            builder.HasIndex(a => new { a.FloorId, a.ApartmentName }).IsUnique();

            // Store Layout as JSONB in PostgreSQL
            builder.Property(a => a.Layout)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                    v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<ApartmentLayout>(v, JsonOptions)
                );
        }
    }
}