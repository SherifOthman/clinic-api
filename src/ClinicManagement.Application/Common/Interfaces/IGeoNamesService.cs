using ClinicManagement.Application.DTOs;

namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// GeoNames proxy service with caching
/// Backend acts as proxy between frontend and GeoNames API
/// Frontend NEVER calls GeoNames directly
/// </summary>
public interface IGeoNamesService
{
    /// <summary>
    /// Get all countries from GeoNames (cached for 24 hours)
    /// </summary>
    Task<List<GeoNamesCountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get states for a country from GeoNames (cached for 24 hours)
    /// </summary>
    Task<List<GeoNamesLocationDto>> GetStatesAsync(int countryGeonameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cities for a state from GeoNames (cached for 24 hours)
    /// </summary>
    Task<List<GeoNamesLocationDto>> GetCitiesAsync(int stateGeonameId, CancellationToken cancellationToken = default);
}
