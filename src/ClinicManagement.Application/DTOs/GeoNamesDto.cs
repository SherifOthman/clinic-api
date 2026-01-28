namespace ClinicManagement.Application.DTOs;

// Simplified GeoNames response - only what we need
public class GeoNamesLocationDto
{
    public int GeoNameId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? AdminName1 { get; set; } // State/Province name
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}

// Simplified search request
public class GeoNamesSearchRequest
{
    public string Query { get; set; } = string.Empty;
    public string? CountryCode { get; set; }
    public string? FeatureClass { get; set; }
    public string? FeatureCode { get; set; }
    public int MaxResults { get; set; } = 50;
    public string? AdminCode1 { get; set; }
}
