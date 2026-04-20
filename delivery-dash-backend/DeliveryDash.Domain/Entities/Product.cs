namespace DeliveryDash.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool InStock { get; set; }
        public bool IsWeightable { get; set; }
        public string ProductImageUrl { get; set; } = string.Empty;

        public Vendor Vendor { get; set; } = null!;
        public Category? Category { get; set; }
    }
}
