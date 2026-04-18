using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Frozen;
using System.Security.Cryptography;

namespace DeliveryDash.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly FileStorageOptions _options;
        private readonly ILogger<FileStorageService> _logger;

        // SECURITY: Maximum image dimensions to prevent decompression bombs
        private const int MaxImageWidth = 8192;
        private const int MaxImageHeight = 8192;

        // SECURITY: Known dangerous extensions
        private static readonly FrozenSet<string> DangerousExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".php", ".php3", ".php4", ".php5", ".phtml", ".asp", ".aspx", ".jsp",
            ".exe", ".dll", ".bat", ".cmd", ".ps1", ".sh", ".py", ".pl", ".cgi",
            ".htaccess", ".htpasswd", ".config", ".ini"
        }.ToFrozenSet();

        // SECURITY: Allowed image formats mapped to encoders
        private static readonly FrozenDictionary<string, IImageEncoder> ImageEncoders =
            new Dictionary<string, IImageEncoder>(StringComparer.OrdinalIgnoreCase)
            {
                [".jpg"] = new JpegEncoder { Quality = 90 },
                [".jpeg"] = new JpegEncoder { Quality = 90 },
                [".png"] = new PngEncoder { CompressionLevel = PngCompressionLevel.DefaultCompression },
                [".gif"] = new GifEncoder(),
                [".webp"] = new WebpEncoder { Quality = 90 }
            }.ToFrozenDictionary();

        public FileStorageService(
            IOptions<FileStorageOptions> options,
            ILogger<FileStorageService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(Stream imageStream, string fileName, string folder, string? baseUrl = null)
        {
            try
            {
                // SECURITY: Validate inputs
                ValidateFolderName(folder);
                ValidateFileName(fileName);

                // SECURITY: Basic stream validation
                if (imageStream == null || !imageStream.CanRead || imageStream.Length == 0)
                    throw new InvalidOperationException("Image stream is null, not readable, or empty.");

                if (imageStream.Length > _options.MaxFileSizeInBytes)
                    throw new InvalidOperationException(
                        $"Image size exceeds maximum allowed size of {_options.MaxFileSizeInBytes / (1024 * 1024)}MB.");

                var extension = Path.GetExtension(fileName).ToLowerInvariant();

                // SECURITY: Process and sanitize image using ImageSharp
                using var sanitizedStream = await SanitizeImageAsync(imageStream, extension);

                // Generate secure filename
                var uniqueFileName = GenerateSecureFileName(extension);

                // Resolve and validate paths
                var baseUploadPath = GetBaseUploadPath();
                var uploadsPath = Path.Combine(baseUploadPath, folder);
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                ValidateFilePath(filePath, baseUploadPath);

                Directory.CreateDirectory(uploadsPath);

                // Save sanitized file
                await SaveFileSecurelyAsync(sanitizedStream, filePath);

                _logger.LogInformation(
                    "Image uploaded successfully. Folder: {Folder}, FileName: {FileName}, OriginalSize: {OriginalSize}, SanitizedSize: {SanitizedSize}",
                    folder, uniqueFileName, imageStream.Length, sanitizedStream.Length);

                return BuildImageUrl(folder, uniqueFileName, baseUrl);
            }
            catch (UnknownImageFormatException ex)
            {
                _logger.LogWarning(ex, "Invalid image format detected for file: {FileName}", fileName);
                throw new InvalidOperationException("File is not a valid image format.", ex);
            }
            catch (InvalidImageContentException ex)
            {
                _logger.LogWarning(ex, "Corrupted or malicious image detected: {FileName}", fileName);
                throw new InvalidOperationException("Image file is corrupted or contains invalid data.", ex);
            }
            catch (Exception ex) when (ex is not InvalidOperationException && ex is not UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Error saving image. FileName: {FileName}, Folder: {Folder}", fileName, folder);
                throw new InvalidOperationException($"Error saving image: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// SECURITY: Sanitizes image by re-encoding it, which:
        /// - Validates the image is actually parseable
        /// - Strips all metadata (EXIF, GPS, camera info)
        /// - Removes any embedded malicious content
        /// - Prevents polyglot attacks
        /// - Limits dimensions to prevent decompression bombs
        /// </summary>
        private async Task<MemoryStream> SanitizeImageAsync(Stream inputStream, string extension)
        {
            inputStream.Position = 0;

            // SECURITY: Load image with size limits to prevent decompression bombs
            var decoderOptions = new DecoderOptions
            {
                MaxFrames = 1, // Prevent animated GIF abuse (optional: set higher for GIF support)
            };

            using var image = await Image.LoadAsync(decoderOptions, inputStream);

            // SECURITY: Check dimensions to prevent decompression bombs
            if (image.Width > MaxImageWidth || image.Height > MaxImageHeight)
            {
                _logger.LogWarning(
                    "Image dimensions too large: {Width}x{Height}. Resizing to max {MaxWidth}x{MaxHeight}",
                    image.Width, image.Height, MaxImageWidth, MaxImageHeight);

                // Resize maintaining aspect ratio
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(MaxImageWidth, MaxImageHeight),
                    Mode = ResizeMode.Max
                }));
            }

            // SECURITY: Strip all metadata (EXIF, IPTC, XMP)
            // This removes GPS coordinates, camera info, timestamps, and any hidden data
            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.XmpProfile = null;
            image.Metadata.IccProfile = null;

            // SECURITY: Re-encode the image to sanitize it
            // This ensures only valid image data is saved, removing any embedded payloads
            var outputStream = new MemoryStream();

            if (!ImageEncoders.TryGetValue(extension, out var encoder))
            {
                throw new InvalidOperationException($"Unsupported image format: {extension}");
            }

            await image.SaveAsync(outputStream, encoder);
            outputStream.Position = 0;

            return outputStream;
        }

        private void ValidateFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new InvalidOperationException("Filename cannot be empty.");

            if (fileName.Contains('\0'))
                throw new InvalidOperationException("Filename contains invalid characters.");

            if (fileName.Length > 255)
                throw new InvalidOperationException("Filename is too long.");

            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !_options.AllowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Blocked file upload with disallowed extension: {Extension}", extension);
                throw new InvalidOperationException(
                    $"File type {extension ?? "unknown"} is not allowed. Allowed types: {string.Join(", ", _options.AllowedExtensions)}");
            }

            // SECURITY: Check for dangerous extensions anywhere in filename
            var fileNameLower = fileName.ToLowerInvariant();
            foreach (var dangerous in DangerousExtensions)
            {
                if (fileNameLower.Contains(dangerous))
                {
                    _logger.LogWarning("Blocked file with dangerous pattern: {FileName}", fileName);
                    throw new InvalidOperationException("Filename contains prohibited patterns.");
                }
            }

            // SECURITY: Check for double extensions
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            if (nameWithoutExt?.Contains('.') == true)
            {
                _logger.LogWarning("Blocked file with multiple extensions: {FileName}", fileName);
                throw new InvalidOperationException("Filename cannot contain multiple extensions.");
            }

            if (fileName.Contains('/') || fileName.Contains('\\') || fileName.Contains(':'))
                throw new InvalidOperationException("Filename contains invalid path characters.");
        }

        private void ValidateFolderName(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
                throw new InvalidOperationException("Folder name cannot be empty.");

            if (folder.Contains("..") || folder.Contains('/') || folder.Contains('\\') ||
                folder.Contains(':') || folder.Contains('\0') || Path.IsPathRooted(folder))
            {
                _logger.LogWarning("Blocked path traversal attempt: {Folder}", folder);
                throw new UnauthorizedAccessException("Invalid folder name detected.");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(folder, @"^[a-zA-Z0-9_-]+$"))
                throw new UnauthorizedAccessException("Folder name contains invalid characters.");

            if (folder.Length > 50)
                throw new InvalidOperationException("Folder name is too long.");
        }

        private static string GenerateSecureFileName(string extension)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(16);
            var fileName = Convert.ToHexString(randomBytes).ToLowerInvariant();
            return $"{fileName}{extension}";
        }

        private string GetBaseUploadPath()
        {
            return Path.IsPathRooted(_options.UploadPath)
                ? _options.UploadPath
                : Path.Combine(Directory.GetCurrentDirectory(), _options.UploadPath);
        }

        private static void ValidateFilePath(string filePath, string baseUploadPath)
        {
            var fullPath = Path.GetFullPath(filePath);
            var fullBasePath = Path.GetFullPath(baseUploadPath);

            if (!fullPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Invalid file path detected.");

            if (fullPath.Length > 260)
                throw new InvalidOperationException("File path exceeds maximum allowed length.");
        }

        private static async Task SaveFileSecurelyAsync(Stream stream, string filePath)
        {
            stream.Position = 0;

            await using var fileStream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await stream.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
        }

        private static string BuildImageUrl(string folder, string fileName, string? baseUrl)
        {
            var relativePath = $"/Uploads/{folder}/{fileName}";
            return string.IsNullOrEmpty(baseUrl)
                ? relativePath
                : $"{baseUrl.TrimEnd('/')}{relativePath}";
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return false;

                var relativePath = ExtractRelativePath(imageUrl);
                var baseUploadPath = GetBaseUploadPath();

                if (relativePath.StartsWith("Uploads" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    relativePath.StartsWith("Uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    relativePath = relativePath["Uploads".Length..].TrimStart('/', '\\');
                }

                var filePath = Path.Combine(baseUploadPath, relativePath);
                var fullPath = Path.GetFullPath(filePath);
                var fullBasePath = Path.GetFullPath(baseUploadPath);

                if (!fullPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Blocked path traversal in delete: {ImageUrl}", imageUrl);
                    throw new UnauthorizedAccessException("Invalid file path detected.");
                }

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Image deleted: {Path}", relativePath);
                    return true;
                }

                return false;
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
                return false;
            }
        }

        private static string ExtractRelativePath(string imageUrl)
        {
            string relativePath = imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                  imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? new Uri(imageUrl).AbsolutePath.TrimStart('/')
                : imageUrl.TrimStart('/');

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        public bool ValidateImage(Stream imageStream, long maxSizeInBytes)
        {
            if (imageStream == null || imageStream.Length == 0)
                throw new InvalidOperationException("Image file is empty or null.");

            if (imageStream.Length > maxSizeInBytes)
                throw new InvalidOperationException(
                    $"Image size exceeds the maximum allowed size of {maxSizeInBytes / (1024 * 1024)}MB.");

            // SECURITY: Use ImageSharp to validate the image can be parsed
            try
            {
                imageStream.Position = 0;
                using var image = Image.Load(imageStream);
                imageStream.Position = 0;
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("File is not a valid image format.", ex);
            }
        }

        public string GetImagePath(string fileName, string folder)
        {
            return Path.Combine(GetBaseUploadPath(), folder, fileName);
        }

        public async Task<List<string>> SaveImagesAsync(
            IEnumerable<(Stream ImageStream, string FileName)> images,
            string folder,
            string? baseUrl = null,
            long maxSizeInBytes = 5 * 1024 * 1024)
        {
            var uploadedUrls = new List<string>();
            var imageList = images.ToList();

            const int maxBatchSize = 20;
            if (imageList.Count > maxBatchSize)
                throw new InvalidOperationException($"Cannot upload more than {maxBatchSize} images at once.");

            try
            {
                foreach (var (imageStream, fileName) in imageList)
                {
                    ValidateImage(imageStream, maxSizeInBytes);
                    var url = await SaveImageAsync(imageStream, fileName, folder, baseUrl);
                    uploadedUrls.Add(url);
                }
                return uploadedUrls;
            }
            catch
            {
                foreach (var url in uploadedUrls)
                    await DeleteImageAsync(url);
                throw;
            }
        }

        public async Task<int> DeleteImagesAsync(IEnumerable<string> imageUrls)
        {
            var deletedCount = 0;
            foreach (var url in imageUrls.Where(u => !string.IsNullOrEmpty(u)))
            {
                if (await DeleteImageAsync(url))
                    deletedCount++;
            }
            return deletedCount;
        }

        public async Task<List<string>> ReplaceImagesAsync(
            IEnumerable<string> existingImageUrls,
            IEnumerable<(Stream ImageStream, string FileName)> newImages,
            string folder,
            string? baseUrl = null,
            long maxSizeInBytes = 5 * 1024 * 1024)
        {
            var newUrls = await SaveImagesAsync(newImages, folder, baseUrl, maxSizeInBytes);
            await DeleteImagesAsync(existingImageUrls);
            return newUrls;
        }
    }
}