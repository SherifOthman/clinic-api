using Microsoft.AspNetCore.Http;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file and returns the file path/URL
    /// </summary>
    Task<string> UploadFileAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a file by its path
    /// </summary>
    Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the full file path from a relative path
    /// </summary>
    string GetFullPath(string relativePath);
    
    /// <summary>
    /// Validates file type and size
    /// </summary>
    bool ValidateFile(IFormFile file, string[] allowedExtensions, long maxSizeInBytes);
}
