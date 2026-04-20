namespace DeliveryDash.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? SortOrder { get; set; }

        public Vendor Vendor { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
