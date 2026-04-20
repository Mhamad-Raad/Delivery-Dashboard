namespace DeliveryDash.Application.Responses.CategoryResponses
{
    public class CategoryDetailResponse
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? SortOrder { get; set; }
        public int ProductsCount { get; set; }
    }
}
