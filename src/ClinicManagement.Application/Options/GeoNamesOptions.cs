namespace ClinicManagement.Application.Options;

/// <summary>
/// GeoNames API configuration options
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

    /// <summary>
    /// Cache duration for countries data
    /// </summary>
    public TimeSpan CountriesCacheDuration { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// Cache duration for states data
    /// </summary>
    public TimeSpan StatesCacheDuration { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Cache duration for cities data
    /// </summary>
    public TimeSpan CitiesCacheDuration { get; set; } = TimeSpan.FromDays(1);
}
