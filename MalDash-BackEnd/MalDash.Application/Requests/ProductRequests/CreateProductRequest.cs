using System.Text.Json.Serialization;

namespace MalDash.Application.Requests.ProductRequests
{
    public class CreateProductRequest
    {
        public int? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool InStock { get; set; }
        public bool IsWeightable { get; set; }
    }
}