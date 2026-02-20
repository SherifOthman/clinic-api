namespace ClinicManagement.Application.Common.Models;

public class ErrorItem
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ApiProblemDetails
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Detail { get; set; }
    public List<ErrorItem>? Errors { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public string? TraceId { get; set; }
}
