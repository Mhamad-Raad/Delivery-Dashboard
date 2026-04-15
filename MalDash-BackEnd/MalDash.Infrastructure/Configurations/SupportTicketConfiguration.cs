using MalDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MalDash.Infrastructure.Configurations
{
    public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
    {
        public void Configure(EntityTypeBuilder<SupportTicket> builder)
        {
            builder.ToTable("SupportTickets");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.TicketNumber).IsRequired().HasMaxLength(50);
            builder.Property(t => t.Subject).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Description).IsRequired().HasMaxLength(2000);
            builder.Property(t => t.AdminNotes).HasMaxLength(1000);

            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Images are loaded manually - not a real FK relationship
            builder.Ignore(t => t.Images);

            builder.HasIndex(t => t.TicketNumber).IsUnique();
            builder.HasIndex(t => t.UserId);
            builder.HasIndex(t => t.Status);
            builder.HasIndex(t => t.CreatedAt);
        }
    }
}