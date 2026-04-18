using DeliveryDash.Domain.Entities.FloorPlan;

namespace DeliveryDash.Domain.Entities
{
    public class Apartment
    {
        public int Id { get; set; }
        public string ApartmentName { get; set; } = string.Empty;
        public int FloorId { get; set; }
        public Floor Floor { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        
        // Stores the floor plan layout as JSON
        public ApartmentLayout? Layout { get; set; }
    }
}