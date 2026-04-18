namespace DeliveryDash.Application.Responses.ProductResponses
{
    public class ProductDetailResponse
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool InStock { get; set; }
        public bool IsWeightable { get; set; }
        public string? ProductImageUrl { get; set; }
    }
}