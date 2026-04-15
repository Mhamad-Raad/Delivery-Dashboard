namespace MalDash.Application.Options
{
    public class FileStorageOptions
    {
        public const string SectionName = "FileStorage";
        
        public string UploadPath { get; set; } = string.Empty;
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
        public long MaxFileSizeInBytes { get; set; }
    }
}