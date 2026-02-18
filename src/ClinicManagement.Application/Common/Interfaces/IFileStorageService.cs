using Microsoft.AspNetCore.Http;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileWithValidationAsync(IFormFile file, string fileType, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}
