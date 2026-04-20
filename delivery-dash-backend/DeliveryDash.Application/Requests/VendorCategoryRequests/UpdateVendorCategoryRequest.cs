namespace DeliveryDash.Application.Requests.VendorCategoryRequests
{
    public class UpdateVendorCategoryRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
