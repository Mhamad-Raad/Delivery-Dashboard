namespace DeliveryDash.API.Extensions
{
    public static class FormFileExtensions
    {
        /// <summary>
        /// Converts a single IFormFile to a tuple for the image upload service.
        /// </summary>
        public static (Stream ImageStream, string FileName)? ToImageUpload(this IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            return (file.OpenReadStream(), file.FileName);
        }

        /// <summary>
        /// Converts multiple IFormFiles to tuples for the image upload service.
        /// </summary>
        public static IEnumerable<(Stream ImageStream, string FileName)> ToImageUploads(
            this IEnumerable<IFormFile>? files)
        {
            if (files == null)
                yield break;

            foreach (var file in files.Where(f => f.Length > 0))
            {
                yield return (file.OpenReadStream(), file.FileName);
            }
        }

        /// <summary>
        /// Gets the base URL from the current HTTP request.
        /// </summary>
        public static string GetBaseUrl(this HttpRequest request)
        {
            return $"{request.Scheme}://{request.Host}";
        }
    }
}