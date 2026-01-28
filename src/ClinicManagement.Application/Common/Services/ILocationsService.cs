using ClinicManagement.Application.DTOs;

namespace ClinicManagement.Application.Common.Services;

public interface ILocationsService
{
    Task<List<CountryDto>> GetCountriesAsync();
    Task<List<StateDto>> GetStatesAsync(int countryId);
    Task<List<CityDto>> GetCitiesAsync(int countryId, int? stateId = null);
    Task<List<CityDto>> SearchCitiesAsync(string countryCode, string query);
    Task<CountryDto?> GetCountryByCodeAsync(string countryCode);
}
