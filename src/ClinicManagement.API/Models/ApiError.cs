using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.API.Models;

/// <summary>
/// Standardized API error response
/// </summary>
public class ApiError
{
    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Field-specific errors (for validation)
    /// </summary>
    public List<ErrorItem>? Errors { get; set; }

    public ApiError(string message)
    {
        Message = message;
    }

    public ApiError(string message, List<ErrorItem> errors)
    {
        Message = message;
        Errors = errors;
    }
}
