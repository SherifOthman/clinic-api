namespace ClinicManagement.Application.DTOs;

public class GeoNamesHealthDto
{
    public bool WebServiceAvailable { get; set; }
    public string? WebServiceResponseTime { get; set; }
    public DateTime? LastSuccessfulSync { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();
}
