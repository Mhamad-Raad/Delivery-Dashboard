namespace DeliveryDash.Application.Requests.VendorRequests
{
    public class UploadVendorImageRequest
    {
        public required Stream ImageStream { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required long FileSize { get; set; }
    }
}