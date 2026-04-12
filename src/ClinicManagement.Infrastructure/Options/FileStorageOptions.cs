namespace ClinicManagement.Infrastructure.Options;

public class FileStorageOptions
{
    public const string Section = "FileStorage";

    public string UploadPath { get; set; } = "uploads";
    public string BaseUrl { get; set; } = "/uploads";
    public Dictionary<string, FileTypeSettings> FileTypes { get; set; } = new();
}

public class FileTypeSettings
{
    public string[] AllowedExtensions { get; set; } = [];
    public long MaxFileSizeBytes { get; set; }
    public string Folder { get; set; } = string.Empty;
}

public static class FileTypes
{
    public const string ProfileImage = "ProfileImage";
    public const string MedicalFile  = "MedicalFile";
}
