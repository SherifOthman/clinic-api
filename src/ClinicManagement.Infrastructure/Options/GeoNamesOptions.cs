namespace ClinicManagement.Infrastructure.Options;

public class GeoNamesOptions
{
    public const string Section = "GeoNames";

    /// <summary>Base URL for GeoNames dump file downloads.</summary>
    public string BaseUrl { get; set; } = "https://download.geonames.org/export/dump";
}
