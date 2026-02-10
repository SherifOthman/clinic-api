using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.API.Models;

public class ApiError
{
    public string Code { get; set; } = string.Empty;

    public List<ErrorItem>? Errors { get; set; }

    public ApiError(string code)
    {
        Code = code;
    }

    public ApiError(string code, List<ErrorItem> errors)
    {
        Code = code;
        Errors = errors;
    }
}
