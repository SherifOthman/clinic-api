namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// Runtime location reference resolver for GeoNames snapshot architecture
/// 
/// IMPORTANT: This is NOT database seeding or startup initialization.
/// This service resolves location references ON-DEMAND during user-triggered flows only.
/// 
/// Behavior:
/// - Runs ONLY at runtime when explicitly invoked by application services
/// - Triggered by user actions (registration, profile update, onboarding, etc.)
/// - Checks if location exists by GeonameId, inserts snapshot ONLY if missing
/// - Does NOT preload, populate, or seed data at startup
/// - Does NOT require GeoNames availability at application startup
/// - GeoNames is an external, read-only provider (not primary data source)
/// 
/// Architecture:
/// - Snapshot pattern: Local copies of GeoNames data for fast queries
/// - On-demand resolution: Location records created strictly when needed
/// - Idempotent: Safe to call multiple times with same GeonameId
/// - Transaction-safe: Wraps operations in database transaction
/// </summary>
public interface ILocationReferenceService
{
    /// <summary>
    /// Resolves location hierarchy at runtime (Country -> State -> City)
    /// Creates local snapshot references ONLY if they don't already exist
    /// 
    /// This is invoked during user-triggered flows (NOT at startup or migration time)
    /// </summary>
    /// <param name="countryGeonameId">GeoNames ID for country</param>
    /// <param name="countryData">Country data from client (Iso2Code, PhoneCode, NameEn, NameAr)</param>
    /// <param name="stateGeonameId">GeoNames ID for state</param>
    /// <param name="stateData">State data from client (NameEn, NameAr)</param>
    /// <param name="cityGeonameId">GeoNames ID for city</param>
    /// <param name="cityData">City data from client (NameEn, NameAr)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The City ID to be used for foreign key relationships</returns>
    Task<int> ResolveLocationAsync(
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
