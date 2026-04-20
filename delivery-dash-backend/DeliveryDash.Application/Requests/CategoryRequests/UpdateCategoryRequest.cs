namespace DeliveryDash.Application.Requests.CategoryRequests
{
    public class UpdateCategoryRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? SortOrder { get; set; }
    }
}
