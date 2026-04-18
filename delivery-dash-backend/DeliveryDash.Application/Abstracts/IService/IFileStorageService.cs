namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IFileStorageService
    {
        Task<string> SaveImageAsync(Stream imageStream, string fileName, string folder, string? baseUrl = null);
        Task<bool> DeleteImageAsync(string imageUrl);
        bool ValidateImage(Stream imageStream, long maxSizeInBytes);
        string GetImagePath(string fileName, string folder);

        Task<List<string>> SaveImagesAsync(
            IEnumerable<(Stream ImageStream, string FileName)> images,
            string folder,
            string? baseUrl = null,
            long maxSizeInBytes = 5 * 1024 * 1024);

        Task<int> DeleteImagesAsync(IEnumerable<string> imageUrls);

        Task<List<string>> ReplaceImagesAsync(
            IEnumerable<string> existingImageUrls,
            IEnumerable<(Stream ImageStream, string FileName)> newImages,
            string folder,
            string? baseUrl = null,
            long maxSizeInBytes = 5 * 1024 * 1024);
    }
}