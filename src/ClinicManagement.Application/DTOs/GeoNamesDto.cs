namespace ClinicManagement.Application.DTOs;

/// <summary>
/// Country DTO from GeoNames API
/// </summary>
public class GeoNamesCountryDto
{
    public int GeonameId { get; set; }
    public string CountryCode { get; set; } = null!; // ISO2
    public string CountryName { get; set; } = null!;
    public string? CountryNameAr { get; set; }
}

/// <summary>
/// Location DTO from GeoNames children API (states/cities)
/// </summary>
public class GeoNamesLocationDto
{
    public int GeonameId { get; set; }
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string Fcode { get; set; } = null!; // Feature code
}

/// <summary>
/// GeoNames API response wrapper
/// </summary>
public class GeoNamesResponse<T>
{
    public List<T> Geonames { get; set; } = new();
}
