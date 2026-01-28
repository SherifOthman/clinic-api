namespace ClinicManagement.Application.Options;

public class GeoNamesOptions
{
    public const string SectionName = "GeoNames";

    public string? Username { get; set; }
    public string BaseUrl { get; set; } = "http://api.geonames.org";
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int DefaultMaxResults { get; set; } = 50;
}
