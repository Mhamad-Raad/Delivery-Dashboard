namespace MalDash.Application.Requests.ProductRequests
{
    public class UpdateProductRequest
    {
        public int? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool? InStock { get; set; }
        public bool? IsWeightable { get; set; }
    }
}