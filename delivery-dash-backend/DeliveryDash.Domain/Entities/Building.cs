namespace DeliveryDash.Domain.Entities
{
    public class Building
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Floor> Floors { get; set; } = new List<Floor>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}