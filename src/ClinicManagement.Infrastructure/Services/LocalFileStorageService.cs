using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ClinicManagement.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly string _uploadPath;
    private readonly string _baseUrl;
    private readonly long _maxFileSize;
    private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly string[] _allowedImageContentTypes = { 
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" 
    };

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger, IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _uploadPath = configuration.GetValue<string>("FileStorage:UploadPath") ?? "wwwroot/uploads";
        _baseUrl = configuration.GetValue<string>("FileStorage:BaseUrl") ?? "/uploads";
        _maxFileSize = configuration.GetValue<long?>("FileStorage:MaxFileSizeBytes") ?? 5L * 1024 * 1024; // 5MB default

        // Ensure upload directory exists
        if (!_fileSystem.DirectoryExists(_uploadPath))
        {
            _fileSystem.CreateDirectory(_uploadPath);
        }
    }

    public async Task<Result<FileUploadResult>> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        string contentType, 
        string folder = "uploads",
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (fileStream.Length > _maxFileSize)
            {
                return Result<FileUploadResult>.Fail($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB");
            }

            if (!IsValidImageFile(fileName, contentType))
            {
                return Result<FileUploadResult>.Fail("Invalid file type. Only image files are allowed.");
            }

            var folderPath = _fileSystem.Combine(_uploadPath, folder);
            if (!_fileSystem.DirectoryExists(folderPath))
            {
                _fileSystem.CreateDirectory(folderPath);
            }

            // Generate unique filename to avoid conflicts
            var fileExtension = _fileSystem.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = _fileSystem.Combine(folderPath, uniqueFileName);
            var relativePath = _fileSystem.Combine(folder, uniqueFileName).Replace('\\', '/');

            using var fileStreamOutput = _fileSystem.CreateFileStream(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

            var result = new FileUploadResult
            {
                FilePath = relativePath,
                FileName = uniqueFileName,
                FileUrl = GetFileUrl(relativePath),
                FileSize = fileStream.Length,
                ContentType = contentType
            };

            _logger.LogInformation("File uploaded successfully: {FileName} to {FilePath}", fileName, relativePath);
            return Result<FileUploadResult>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            return Result<FileUploadResult>.Fail("Failed to upload file");
        }
    }

    public async Task<Result> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = _fileSystem.Combine(_uploadPath, filePath);
            
            if (_fileSystem.Exists(fullPath))
            {
                await Task.Run(() => _fileSystem.DeleteFile(fullPath), cancellationToken);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return Result.Fail(ApplicationErrors.File.FILE_DELETE_FAILED);
        }
    }

    public async Task<Result<Stream>> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = _fileSystem.Combine(_uploadPath, filePath);
            
            if (!_fileSystem.Exists(fullPath))
            {
            return Result<Stream>.Fail(ApplicationErrors.File.FILE_NOT_FOUND);
            }

            var stream = _fileSystem.CreateFileStream(fullPath, FileMode.Open, FileAccess.Read);
            return Result<Stream>.Ok(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file: {FilePath}", filePath);
            return Result<Stream>.Fail("Failed to get file");
        }
    }

    public bool IsValidImageFile(string fileName, string contentType)
    {
        var extension = _fileSystem.GetExtension(fileName).ToLowerInvariant();
        return _allowedImageExtensions.Contains(extension) && 
               _allowedImageContentTypes.Contains(contentType.ToLowerInvariant());
    }

    public string GetFileUrl(string filePath)
    {
        return $"{_baseUrl.TrimEnd('/')}/{filePath.TrimStart('/')}";
    }
}