namespace MalDash.Domain.Models
{
    public record DriverQueueEntry
    {
        public Guid DriverId { get; init; }
        public DateTime EnqueuedAt { get; init; }
        public int ShiftId { get; init; }
        public int Priority { get; init; } = 0; // For future priority handling
    }
}