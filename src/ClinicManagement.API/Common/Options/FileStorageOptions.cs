namespace ClinicManagement.API.Common.Options;

/// <summary>
/// File storage configuration options
/// </summary>
public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Upload path relative to web root
    /// </summary>
    public string UploadPath { get; set; } = "wwwroot/uploads";

    /// <summary>
    /// Base URL for accessing uploaded files
    /// </summary>
    public string BaseUrl { get; set; } = "/uploads";

    /// <summary>
    /// Maximum file size in bytes (default: 5MB)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 5242880;

    /// <summary>
    /// Profile image specific settings
    /// </summary>
    public ProfileImageSettings ProfileImage { get; set; } = new();
}

public class ProfileImageSettings
{
    /// <summary>
    /// Allowed file extensions for profile images
    /// </summary>
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif" };

    /// <summary>
    /// Maximum file size in bytes for profile images (default: 5MB)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 5242880;

    /// <summary>
    /// Folder name for storing profile images
    /// </summary>
    public string Folder { get; set; } = "profiles";
}
