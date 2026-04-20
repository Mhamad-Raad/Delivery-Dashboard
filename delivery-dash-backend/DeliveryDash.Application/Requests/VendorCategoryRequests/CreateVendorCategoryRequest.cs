namespace DeliveryDash.Application.Requests.VendorCategoryRequests
{
    public class CreateVendorCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
