namespace ClinicManagement.Application.Abstractions.Services;

/// <summary>
/// Downloads and parses GeoNames bulk data files.
/// All files are cached on disk after first download.
/// Upload cities_processed.tsv to the server to skip re-download.
/// </summary>
public interface IGeoNamesService
{
    Task<List<GeoNamesCountryDump>> GetCountriesAsync(CancellationToken ct = default);
    Task<List<GeoNamesStateDump>>   GetStatesAsync(CancellationToken ct = default);
    Task<List<GeoNamesCityDump>>    GetCitiesAsync(CancellationToken ct = default);
}

public record GeoNamesCountryDump(int GeonameId, string CountryCode, string NameEn, string NameAr);
public record GeoNamesStateDump(int GeonameId, int CountryGeonameId, string NameEn, string NameAr);
public record GeoNamesCityDump(int GeonameId, int StateGeonameId, string NameEn, string NameAr);
