namespace ClinicManagement.Infrastructure.Options;

public class GeoNamesOptions
{
    public const string Section = "GeoNames";

    public string Username { get; set; } = null!;
    public string BaseUrl  { get; set; } = "https://secure.geonames.org";

    /// <summary>
    /// City filter configuration — controls which populated places are included during seed.
    /// Change these in appsettings.json and re-run the seed endpoint; no code change needed.
    /// </summary>
    public CityFilterOptions CityFilter { get; set; } = new();
}

public class CityFilterOptions
{
    /// <summary>
    /// GeoNames feature codes to always include regardless of population.
    /// Defaults: PPLC (capital), PPLA (admin seat 1), PPLA2, PPLA3, PPLA4.
    /// See https://www.geonames.org/export/codes.html for all codes.
    /// </summary>
    public List<string> AlwaysIncludeFeatureCodes { get; set; } =
        ["PPLC", "PPLA", "PPLA2", "PPLA3", "PPLA4"];

    /// <summary>
    /// Minimum population for PPL (generic populated place) feature code.
    /// Set to 0 to include all populated places (may be very large).
    /// Default: 50000.
    /// </summary>
    public int MinPopulationForPpl { get; set; } = 50_000;

    /// <summary>Max rows per GeoNames search request. GeoNames free limit is 1000.</summary>
    public int MaxRows { get; set; } = 1000;
}
