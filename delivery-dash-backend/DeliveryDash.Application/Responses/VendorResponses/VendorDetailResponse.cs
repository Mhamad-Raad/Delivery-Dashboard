namespace DeliveryDash.Application.Responses.VendorResponses
{
    public class VendorDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? UserProfileImageUrl { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public int VendorCategoryId { get; set; }
        public string VendorCategoryName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
