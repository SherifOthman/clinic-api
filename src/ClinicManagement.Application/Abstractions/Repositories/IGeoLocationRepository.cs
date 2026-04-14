namespace ClinicManagement.Application.Abstractions.Repositories;

/// <summary>Read-only access to the seeded GeoNames reference tables.</summary>
public interface IGeoLocationRepository
{
    Task<List<CountryItem>> GetCountriesAsync(CancellationToken ct = default);
    Task<List<StateItem>>   GetStatesAsync(int countryGeonameId, CancellationToken ct = default);
    Task<List<CityItem>>    GetCitiesAsync(int stateGeonameId, CancellationToken ct = default);
}

/// <summary>Both language names are always returned — the frontend picks the one to display.</summary>
public record CountryItem(int GeonameId, string NameEn, string NameAr, string CountryCode);
public record StateItem(int GeonameId, string NameEn, string NameAr);
public record CityItem(int GeonameId, string NameEn, string NameAr);
