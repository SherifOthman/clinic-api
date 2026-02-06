using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Runtime location reference resolver for GeoNames snapshot architecture
/// 
/// CRITICAL: This is NOT database seeding, startup initialization, or migration logic.
/// This service resolves location references ON-DEMAND during user-triggered operations.
/// 
/// When it runs:
/// - During user registration (when user selects location)
/// - During clinic onboarding (when clinic owner sets branch location)
/// - During profile updates (when user changes location)
/// - NEVER at application startup
/// - NEVER during migrations
/// - NEVER as background job
/// 
/// What it does:
/// - Receives GeoNames data from client (after user selected location from GeoNames API)
/// - Checks if Country/State/City already exist locally by GeonameId
/// - Creates local snapshot ONLY if missing (idempotent operation)
/// - Returns CityId for foreign key relationships
/// - Does NOT update existing snapshots
/// - Does NOT fetch from GeoNames at runtime
/// 
/// Architecture principles:
/// - GeoNames is external, read-only provider (client-side integration)
/// - Local snapshots for fast queries and joins
/// - On-demand resolution: Records created strictly when needed by users
/// - Transaction-safe: All operations wrapped in database transaction
/// - Deleting all users does NOT cause re-population
/// - GeoNames availability NOT required at startup
/// </summary>
public class LocationReferenceService : ILocationReferenceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LocationReferenceService> _logger;

    public LocationReferenceService(IUnitOfWork unitOfWork, ILogger<LocationReferenceService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Resolves location hierarchy at runtime during user-triggered operations
    /// Creates local snapshot references ONLY if they don't already exist
    /// 
    /// This method is invoked by:
    /// - Onboarding flow (CompleteOnboardingCommandHandler)
    /// - User registration with location
    /// - Profile update with location change
    /// 
    /// This method is NEVER invoked by:
    /// - Application startup
    /// - Database migrations
    /// - Background jobs
    /// - Scheduled tasks
    /// </summary>
    public async Task<int> ResolveLocationAsync(
        int countryGeonameId,
        CountrySnapshotData countryData,
        int stateGeonameId,
        StateSnapshotData stateData,
        int cityGeonameId,
        CitySnapshotData cityData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting location resolution - Country: {CountryGeonameId}, State: {StateGeonameId}, City: {CityGeonameId}",
            countryGeonameId, stateGeonameId, cityGeonameId);

        // Start transaction for consistency
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Resolve Country (check by GeonameId, create snapshot if missing)
            var country = await _unitOfWork.Countries.GetByGeonameIdAsync(countryGeonameId, cancellationToken);
            if (country == null)
            {
                _logger.LogInformation("Country not found in local database, creating snapshot - GeonameId: {GeonameId}, Name: {NameEn}",
                    countryGeonameId, countryData.NameEn);

                // Insert snapshot from GeoNames data received from client
                country = new Country
                {
                    GeonameId = countryGeonameId,
                    Iso2Code = countryData.Iso2Code,
                    PhoneCode = countryData.PhoneCode,
                    NameEn = countryData.NameEn,
                    NameAr = countryData.NameAr
                };
                await _unitOfWork.Countries.AddAsync(country);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Country snapshot created - Id: {CountryId}, GeonameId: {GeonameId}",
                    country.Id, country.GeonameId);
            }
            else
            {
                _logger.LogDebug("Country already exists in local database - Id: {CountryId}, GeonameId: {GeonameId}",
                    country.Id, country.GeonameId);
            }

            // 2. Resolve State (check by GeonameId, create snapshot if missing)
            var state = await _unitOfWork.States.GetByGeonameIdAsync(stateGeonameId, cancellationToken);
            if (state == null)
            {
                _logger.LogInformation("State not found in local database, creating snapshot - GeonameId: {GeonameId}, Name: {NameEn}",
                    stateGeonameId, stateData.NameEn);

                // Insert snapshot and link to Country
                state = new State
                {
                    GeonameId = stateGeonameId,
                    CountryId = country.Id,
                    NameEn = stateData.NameEn,
                    NameAr = stateData.NameAr
                };
                await _unitOfWork.States.AddAsync(state);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("State snapshot created - Id: {StateId}, GeonameId: {GeonameId}, CountryId: {CountryId}",
                    state.Id, state.GeonameId, state.CountryId);
            }
            else
            {
                _logger.LogDebug("State already exists in local database - Id: {StateId}, GeonameId: {GeonameId}",
                    state.Id, state.GeonameId);
            }

            // 3. Resolve City (check by GeonameId, create snapshot if missing)
            var city = await _unitOfWork.Cities.GetByGeonameIdAsync(cityGeonameId, cancellationToken);
            if (city == null)
            {
                _logger.LogInformation("City not found in local database, creating snapshot - GeonameId: {GeonameId}, Name: {NameEn}",
                    cityGeonameId, cityData.NameEn);

                // Insert snapshot and link to State
                city = new City
                {
                    GeonameId = cityGeonameId,
                    StateId = state.Id,
                    NameEn = cityData.NameEn,
                    NameAr = cityData.NameAr
                };
                await _unitOfWork.Cities.AddAsync(city);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("City snapshot created - Id: {CityId}, GeonameId: {GeonameId}, StateId: {StateId}",
                    city.Id, city.GeonameId, city.StateId);
            }
            else
            {
                _logger.LogDebug("City already exists in local database - Id: {CityId}, GeonameId: {GeonameId}",
                    city.Id, city.GeonameId);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Location resolution completed successfully - CityId: {CityId}", city.Id);

            // Return City ID for foreign key relationships
            return city.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Location resolution failed - Country: {CountryGeonameId}, State: {StateGeonameId}, City: {CityGeonameId}",
                countryGeonameId, stateGeonameId, cityGeonameId);

            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
