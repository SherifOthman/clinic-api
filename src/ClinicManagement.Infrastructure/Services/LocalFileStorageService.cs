using ClinicManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;
    private const string UploadsFolder = "uploads";

    public LocalFileStorageService(IWebHostEnvironment environment, ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            // Create unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            
            // Build folder path: wwwroot/uploads/{folder}
            var folderPath = Path.Combine(_environment.WebRootPath, UploadsFolder, folder);
            
            // Ensure directory exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Full file path
            var filePath = Path.Combine(folderPath, fileName);

            // Save file
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, cancellationToken);

            // Return relative path for storage in database
            var relativePath = $"/{UploadsFolder}/{folder}/{fileName}";
            
            _logger.LogInformation("File uploaded successfully: {FilePath}", relativePath);
            
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            // Remove leading slash if present
            var cleanPath = filePath.TrimStart('/');
            var fullPath = Path.Combine(_environment.WebRootPath, cleanPath);

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath), cancellationToken);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                return true;
            }

            _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return false;
        }
    }

    public string GetFullPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        var cleanPath = relativePath.TrimStart('/');
        return Path.Combine(_environment.WebRootPath, cleanPath);
    }

    public bool ValidateFile(IFormFile file, string[] allowedExtensions, long maxSizeInBytes)
    {
        if (file == null || file.Length == 0)
        {
            return false;
        }

        // Check file size
        if (file.Length > maxSizeInBytes)
        {
            _logger.LogWarning("File size exceeds limit: {Size} bytes", file.Length);
            return false;
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            _logger.LogWarning("File extension not allowed: {Extension}", extension);
            return false;
        }

        return true;
    }
}
