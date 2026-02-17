namespace ClinicManagement.Application.Common.Options;

/// <summary>
/// GeoNames API configuration options
/// Caching is handled by Output Cache at endpoint level (24 hours)
/// </summary>
public class GeoNamesOptions
{
    public const string SectionName = "GeoNames";

    /// <summary>
    /// GeoNames API username
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// GeoNames API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "http://api.geonames.org";
}
