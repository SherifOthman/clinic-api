namespace ClinicManagement.API.Common.Options;

/// <summary>
/// File storage configuration options
/// </summary>
public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Upload path relative to web root (e.g., "uploads")
    /// </summary>
    public string UploadPath { get; set; } = "uploads";

    /// <summary>
    /// Base URL for accessing uploaded files
    /// </summary>
    public string BaseUrl { get; set; } = "/uploads";

    /// <summary>
    /// File type specific settings
    /// </summary>
    public Dictionary<string, FileTypeSettings> FileTypes { get; set; } = new();
}

/// <summary>
/// File type specific settings
/// </summary>
public class FileTypeSettings
{
    /// <summary>
    /// Allowed file extensions
    /// </summary>
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Maximum file size in bytes
    /// </summary>
    public long MaxFileSizeBytes { get; set; }

    /// <summary>
    /// Folder name for storing files
    /// </summary>
    public string Folder { get; set; } = string.Empty;
}

/// <summary>
/// File type constants to avoid typos
/// </summary>
public static class FileTypes
{
    public const string ProfileImage = "ProfileImage";
    public const string MedicalFile = "MedicalFile";
}
