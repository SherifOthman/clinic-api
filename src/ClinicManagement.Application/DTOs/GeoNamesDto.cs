namespace ClinicManagement.Application.DTOs;

/// <summary>
/// Country DTO from GeoNames API with bilingual support
/// </summary>
public class GeoNamesCountryDto
{
    public int GeonameId { get; set; }
    public string CountryCode { get; set; } = null!; // ISO2
    public string PhoneCode { get; set; } = null!; // e.g., "+20"
    public BilingualName Name { get; set; } = null!;
}

/// <summary>
/// Location DTO from GeoNames children API (states/cities) with bilingual support
/// </summary>
public class GeoNamesLocationDto
{
    public int GeonameId { get; set; }
    public BilingualName Name { get; set; } = null!;
    public string Fcode { get; set; } = null!; // Feature code
}

/// <summary>
/// Bilingual name structure for Arabic and English
/// </summary>
public class BilingualName
{
    public string En { get; set; } = null!;
    public string Ar { get; set; } = null!;
}

/// <summary>
/// GeoNames API response wrapper
/// </summary>
public class GeoNamesResponse<T>
{
    public List<T> Geonames { get; set; } = new();
}
