namespace MalDash.Application.Requests.ApartmentRequests.FloorPlan
{
    public record RoomLayoutRequest
    {
        public required string Id { get; init; }
        public required string Type { get; init; }
        public required string Name { get; init; }
        public required double X { get; init; }
        public required double Y { get; init; }
        public required double Width { get; init; }
        public required double Height { get; init; }
    }

    public record DoorRequest
    {
        public required string Id { get; init; }
        public required string RoomId { get; init; }
        public string? ConnectedRoomId { get; init; }
        public required string Edge { get; init; }
        public required double Position { get; init; }
        public required double Width { get; init; }
    }

    public record ApartmentLayoutRequest
    {
        public List<RoomLayoutRequest> Rooms { get; init; } = [];
        public List<DoorRequest> Doors { get; init; } = [];
        public int GridSize { get; init; } = 48;
    }
}