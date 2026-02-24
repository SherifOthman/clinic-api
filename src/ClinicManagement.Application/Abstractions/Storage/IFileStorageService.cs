using ClinicManagement.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace ClinicManagement.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<Result<string>> UploadFileWithValidationAsync(IFormFile file, string fileType, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}
