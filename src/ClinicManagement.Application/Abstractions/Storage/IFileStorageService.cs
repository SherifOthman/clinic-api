using Microsoft.AspNetCore.Http;

namespace ClinicManagement.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<string> UploadFileWithValidationAsync(IFormFile file, string fileType, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}
