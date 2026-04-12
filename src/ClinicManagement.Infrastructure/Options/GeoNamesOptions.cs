namespace ClinicManagement.Infrastructure.Options;

public class GeoNamesOptions
{
    public const string Section = "GeoNames";

    public string Username { get; set; } = null!;
    public string BaseUrl { get; set; } = "http://api.geonames.org";
}
