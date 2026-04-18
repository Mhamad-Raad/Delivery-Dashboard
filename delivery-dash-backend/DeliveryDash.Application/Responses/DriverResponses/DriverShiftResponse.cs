namespace DeliveryDash.Application.Responses.DriverResponses
{
    public class DriverShiftResponse
    {
        public int Id { get; set; }
        public Guid DriverId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool IsActive { get; set; }
    }
}