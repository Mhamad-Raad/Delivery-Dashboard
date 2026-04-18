namespace DeliveryDash.Domain.Entities
{
    public class Floor
    {
        public int Id { get; set; }
        public int FloorNumber { get; set; }
        public int BuildingId { get; set; }
        
        public Building Building { get; set; }
        public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}