using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Application.Common.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly FileStorageOptions _options;

    public LocalFileStorageService(
        IHostEnvironment environment, 
        ILogger<LocalFileStorageService> logger,
        IOptions<FileStorageOptions> options)
    {
        _environment = environment;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var webRootPath = _environment.ContentRootPath; // Use ContentRootPath instead of WebRootPath
            var folderPath = Path.Combine(webRootPath, "wwwroot", _options.UploadPath, folder);
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, cancellationToken);

            var relativePath = $"{_options.BaseUrl}/{folder}/{fileName}";
            
            _logger.LogInformation("File uploaded successfully: {FilePath}", relativePath);
            
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
            throw;
        }
    }

    public async Task<string> UploadFileWithValidationAsync(
        IFormFile file,
        string fileType,
        CancellationToken cancellationToken = default)
    {
        // Get file type settings
        if (!_options.FileTypes.TryGetValue(fileType, out var fileTypeSettings))
        {
            throw new ArgumentException($"File type '{fileType}' is not configured");
        }

        // Validate file
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required");
        }

        if (file.Length > fileTypeSettings.MaxFileSizeBytes)
        {
            var maxSizeMB = fileTypeSettings.MaxFileSizeBytes / (1024 * 1024);
            throw new ArgumentException($"File size must not exceed {maxSizeMB}MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!fileTypeSettings.AllowedExtensions.Contains(extension))
        {
            var allowedTypes = string.Join(", ", fileTypeSettings.AllowedExtensions);
            throw new ArgumentException($"File must be one of the following types: {allowedTypes}");
        }

        return await UploadFileAsync(file, fileTypeSettings.Folder, cancellationToken);
    }

    public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var cleanPath = filePath.TrimStart('/');
        var webRootPath = _environment.ContentRootPath;
        var fullPath = Path.Combine(webRootPath, "wwwroot", cleanPath);

        if (File.Exists(fullPath))
        {
            await Task.Run(() => File.Delete(fullPath), cancellationToken);
            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
        }
        else
        {
            _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
        }
    }

    public string GetFullPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        var cleanPath = relativePath.TrimStart('/');
        var webRootPath = _environment.ContentRootPath;
        return Path.Combine(webRootPath, "wwwroot", cleanPath);
    }

    public void ValidateFile(IFormFile file, string[] allowedExtensions, long maxSizeInBytes)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required");
        }

        if (file.Length > maxSizeInBytes)
        {
            var maxSizeMB = maxSizeInBytes / (1024 * 1024);
            throw new ArgumentException($"File size must not exceed {maxSizeMB}MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            var allowedTypes = string.Join(", ", allowedExtensions);
            throw new ArgumentException($"File must be one of the following types: {allowedTypes}");
        }
    }
}

