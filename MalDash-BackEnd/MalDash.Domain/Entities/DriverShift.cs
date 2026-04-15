namespace MalDash.Domain.Entities
{
    public class DriverShift
    {
        public int Id { get; set; }
        public Guid DriverId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool IsActive => EndedAt == null;

        // Navigation
        public User Driver { get; set; } = null!;
        public ICollection<OrderAssignment> OrderAssignments { get; set; } = [];
    }
}