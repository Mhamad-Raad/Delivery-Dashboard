using System.Text.Json.Serialization;

namespace DeliveryDash.Domain.Entities
{
    public class Address
    {
        public int Id { get; set; }
        
        // Foreign keys to Building structure
        public int? BuildingId { get; set; }
        public int? FloorId { get; set; }
        public int? ApartmentId { get; set; }

        // User relationship
        public Guid? UserId { get; set; }
        
        // Navigation properties
        [JsonIgnore]
        public Building? Building { get; set; }
        
        [JsonIgnore]
        public Floor? Floor { get; set; }
        
        [JsonIgnore]
        public Apartment? Apartment { get; set; }
        
        [JsonIgnore]
        public User? User { get; set; }
    }
}