namespace ClinicManagement.API.Common.Models;

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string? Message { get; set; }

    public ApiError(string code)
    {
        Code = code;
    }

    public ApiError(string code, string message)
    {
        Code = code;
        Message = message;
    }
}
