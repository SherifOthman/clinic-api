namespace ClinicManagement.Application.Abstractions.Services;

/// <summary>Minimal interface for GeoNames data access — used by the seed service.</summary>
public interface IGeoNamesService
{
    Task<List<GeoNamesItem>> GetCountriesAsync(string lang = "en", CancellationToken ct = default);
    Task<List<GeoNamesItem>> GetStatesAsync(int countryGeonameId, string lang = "en", CancellationToken ct = default);
    Task<List<GeoNamesItem>> GetCitiesAsync(int stateGeonameId, string lang = "en", CancellationToken ct = default);
}

public record GeoNamesItem(int GeonameId, string Name, string CountryCode = "");
