using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.API.Models;

public class ApiError
{
    public string Message { get; set; } = string.Empty;

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
