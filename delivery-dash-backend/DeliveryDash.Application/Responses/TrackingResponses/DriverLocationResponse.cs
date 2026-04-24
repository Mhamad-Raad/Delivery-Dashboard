namespace DeliveryDash.Application.Responses.TrackingResponses
{
    public class DriverLocationResponse
    {
        public int OrderId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double? Heading { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
