namespace ClinicManagement.Infrastructure.Options;

public class GeoNamesOptions
{
    public const string Section = "GeoNames";

    /// <summary>
    /// Base URL for GeoNames dump files.
    /// Default: https://download.geonames.org/export/dump
    /// </summary>
    public string BaseUrl { get; set; } = "https://download.geonames.org/export/dump";

    /// <summary>
    /// Local directory where downloaded files are cached.
    /// Relative to ContentRootPath. Default: wwwroot/SeedData/GeoNames
    /// On shared hosting: upload files here via the file manager — they will never be re-downloaded.
    /// </summary>
    public string CacheDir { get; set; } = "wwwroot/SeedData/GeoNames";
}
