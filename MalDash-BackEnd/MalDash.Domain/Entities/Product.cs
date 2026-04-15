namespace MalDash.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int? CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool InStock { get; set; }
        public bool IsWeightable { get; set; }
        public string ProductImageUrl { get; set; }

        // Navigation property
        public Vendor Vendor { get; set; }
        public Catagory? Category { get; set; }
    }
}