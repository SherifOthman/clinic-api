namespace ClinicManagement.Application.Abstractions.Repositories;

/// <summary>Read-only access to the seeded GeoNames reference tables.</summary>
public interface IGeoLocationRepository
{
    Task<List<CountryItem>> GetCountriesAsync(string lang, CancellationToken ct = default);
    Task<List<StateItem>>   GetStatesAsync(int countryGeonameId, string lang, CancellationToken ct = default);
    Task<List<CityItem>>    GetCitiesAsync(int stateGeonameId, string lang, CancellationToken ct = default);
}

public record CountryItem(int GeonameId, string Name, string CountryCode);
public record StateItem(int GeonameId, string Name);
public record CityItem(int GeonameId, string Name);
