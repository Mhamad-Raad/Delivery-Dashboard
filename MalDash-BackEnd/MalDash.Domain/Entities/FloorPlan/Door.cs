namespace MalDash.Domain.Entities.FloorPlan
{
    public class Door
    {
        public string Id { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public string? ConnectedRoomId { get; set; }
        public string Edge { get; set; } = string.Empty;  // "top", "bottom", "left", "right"
        public double Position { get; set; }  // 0-1 along the edge
        public double Width { get; set; }  // Door width in meters
    }
}