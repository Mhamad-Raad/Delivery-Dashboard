namespace MalDash.Application.Responses.ApartmentResponses
{
    public class RoomLayoutResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class DoorResponse
    {
        public string Id { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public string? ConnectedRoomId { get; set; }
        public string Edge { get; set; } = string.Empty;
        public double Position { get; set; }
        public double Width { get; set; }
    }

    public class ApartmentLayoutResponse
    {
        public List<RoomLayoutResponse> Rooms { get; set; } = new();
        public List<DoorResponse> Doors { get; set; } = new();
        public int GridSize { get; set; } = 48;
    }
}