namespace DeliveryDash.Application.Requests.Common
{
    /// <summary>
    /// Universal image upload request that works for any entity.
    /// </summary>
    public class UploadImageRequest
    {
        public required Stream ImageStream { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required long FileSize { get; set; }
    }
}