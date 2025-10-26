using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.API.Models;

public class ApiError
{
    public string Type { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<ErrorItem>? Errors { get; set; }

}
