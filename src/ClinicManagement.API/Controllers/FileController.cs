using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/files")]
public class FileController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public FileController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpGet("{folder}/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFile(string folder, string fileName, CancellationToken cancellationToken)
    {
        var filePath = $"{folder}/{fileName}";
        var result = await _fileStorageService.GetFileAsync(filePath, cancellationToken);

        if (!result.Success || result.Value == null)
        {
            return NotFound(new ApiError(MessageCodes.File.FILE_NOT_FOUND));
        }

        // Determine content type based on file extension
        var contentType = GetContentType(fileName);
        
        return File(result.Value, contentType);
    }

    [HttpDelete("{folder}/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteFile(string folder, string fileName, CancellationToken cancellationToken)
    {
        var filePath = $"{folder}/{fileName}";
        var result = await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.ToApiError());
        }

        return Ok();
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".xml" => "application/xml",
            _ => "application/octet-stream"
        };
    }
}