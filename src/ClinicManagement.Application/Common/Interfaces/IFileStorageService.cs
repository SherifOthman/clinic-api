using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<Result<FileUploadResult>> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        string contentType, 
        string folder = "uploads",
        CancellationToken cancellationToken = default);

    Task<Result> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    Task<Result<Stream>> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    bool IsValidImageFile(string fileName, string contentType);
    
    string GetFileUrl(string filePath);
}

public class FileUploadResult
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
}