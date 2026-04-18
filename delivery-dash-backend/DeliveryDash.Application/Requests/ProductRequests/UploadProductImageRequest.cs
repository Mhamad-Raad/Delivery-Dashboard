namespace DeliveryDash.Application.Requests.ProductRequests
{
    public class UploadProductImageRequest
    {
        public Stream ImageStream { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
    }
}