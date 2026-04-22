namespace ClinicManagement.Application.Abstractions.Services;

/// <summary>
/// Downloads GeoNames bulk data dumps and parses them into seed-ready collections.
/// No API credits or rate limits — uses the free file exports from download.geonames.org.
/// </summary>
public interface IGeoNamesService
{
    /// <summary>Downloads and parses all countries from countryInfo.txt.</summary>
    Task<List<GeoNamesCountryDump>> GetCountriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Downloads and parses admin1CodesASCII.txt for English state names,
    /// and alternateNamesV2 filtered to 'ar' for Arabic names.
    /// Returns a flat list of all ADM1 states worldwide.
    /// </summary>
    Task<List<GeoNamesStateDump>> GetStatesAsync(CancellationToken ct = default);

    /// <summary>
    /// Downloads and parses cities500.zip (population > 500, ~200K cities).
    /// Deduplicates by (state, name) keeping the highest population.
    /// Results are cached to cities_processed.tsv for fast subsequent reads.
    /// </summary>
    Task<List<GeoNamesCityDump>> GetCitiesAsync(CancellationToken ct = default);

    /// <summary>
    /// Streams cities one by one. Delegates to GetCitiesAsync internally.
    /// </summary>
    IAsyncEnumerable<GeoNamesCityDump> StreamCitiesAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the expected total city count stored in the cities_processed.tsv header.
    /// Returns null if the file doesn't exist or has no count in the header.
    /// </summary>
    Task<int?> GetExpectedCityCountAsync(CancellationToken ct = default);
}

public record GeoNamesCountryDump(int GeonameId, string CountryCode, string NameEn, string NameAr);
public record GeoNamesStateDump(int GeonameId, int CountryGeonameId, string NameEn, string NameAr);
public record GeoNamesCityDump(int GeonameId, int StateGeonameId, string NameEn, string NameAr);
