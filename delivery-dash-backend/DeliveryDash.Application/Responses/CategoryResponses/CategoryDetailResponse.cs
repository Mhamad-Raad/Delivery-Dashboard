namespace DeliveryDash.Application.Responses.CategoryResponses
{
    public class CategoryDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public List<CategoryResponse> SubCategories { get; set; } = new();
        public int ProductsCount { get; set; }
    }
}