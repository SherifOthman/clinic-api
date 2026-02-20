namespace ClinicManagement.API.Models;

public class ApiProblemDetails
{
    public string? Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string? Code { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? TraceId { get; set; }
}
