namespace MalDash.Domain.Entities.FloorPlan
{
    public class RoomLayout
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;  // "bedroom", "bathroom", "kitchen", etc.
        public string Name { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}