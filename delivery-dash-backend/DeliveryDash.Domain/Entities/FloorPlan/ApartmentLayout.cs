namespace DeliveryDash.Domain.Entities.FloorPlan
{
    public class ApartmentLayout
    {
        public List<RoomLayout> Rooms { get; set; } = new();
        public List<Door> Doors { get; set; } = new();
        public int GridSize { get; set; } = 48;
    }
}