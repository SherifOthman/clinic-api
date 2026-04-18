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
    /// Downloads cities1000.zip (cities with pop>1000 or PPLA seats) and
    /// alternateNamesV2 filtered to 'ar'. Returns cities grouped by ADM1 geonameId.
    /// </summary>
    Task<IEnumerable<GeoNamesCityDump>> GetCitiesAsync(CancellationToken ct = default);
}

public record GeoNamesCountryDump(int GeonameId, string CountryCode, string NameEn, string NameAr);
public record GeoNamesStateDump(int GeonameId, int CountryGeonameId, string NameEn, string NameAr);
public record GeoNamesCityDump(int GeonameId, int StateGeonameId, string NameEn, string NameAr);
