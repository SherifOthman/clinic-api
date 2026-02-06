namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// Service for lazy seeding location data from GeoNames
/// Implements snapshot architecture: check by GeonameId, insert if not exists
/// </summary>
public interface ILocationSeedingService
{
    /// <summary>
    /// Ensures location hierarchy exists in database (Country -> State -> City)
    /// Uses lazy seeding: checks by GeonameId and inserts snapshot if not found
    /// </summary>
    /// <param name="countryGeonameId">GeoNames ID for country</param>
    /// <param name="countryData">Country data from client (Iso2Code, PhoneCode, NameEn, NameAr)</param>
    /// <param name="stateGeonameId">GeoNames ID for state</param>
    /// <param name="stateData">State data from client (NameEn, NameAr)</param>
    /// <param name="cityGeonameId">GeoNames ID for city</param>
    /// <param name="cityData">City data from client (NameEn, NameAr)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The City ID to be used for foreign key relationships</returns>
    Task<int> EnsureLocationExistsAsync(
        int countryGeonameId,
        CountrySnapshotData countryData,
        int stateGeonameId,
        StateSnapshotData stateData,
        int cityGeonameId,
        CitySnapshotData cityData,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Country snapshot data from GeoNames (received from client)
/// </summary>
public record CountrySnapshotData(string Iso2Code, string PhoneCode, string NameEn, string NameAr);

/// <summary>
/// State snapshot data from GeoNames (received from client)
/// </summary>
public record StateSnapshotData(string NameEn, string NameAr);

/// <summary>
/// City snapshot data from GeoNames (received from client)
/// </summary>
public record CitySnapshotData(string NameEn, string NameAr);
