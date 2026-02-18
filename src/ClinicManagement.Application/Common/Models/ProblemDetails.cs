namespace ClinicManagement.Application.Common.Models;

/// <summary>
/// RFC 7807 Problem Details response format
/// Frontend translates error codes to user's language (Arabic/English)
/// </summary>
public class ApiProblemDetails
{
    /// <summary>Error code for frontend translation (e.g., "INSUFFICIENT_STOCK")</summary>
    public string Code { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    
    /// <summary>Detailed message in English for debugging</summary>
    public string? Detail { get; set; }
    
    /// <summary>Additional data for message interpolation (e.g., { "available": 5, "requested": 10 })</summary>
    public Dictionary<string, object>? Data { get; set; }
    
    /// <summary>Request trace ID for debugging</summary>
    public string? TraceId { get; set; }
}
