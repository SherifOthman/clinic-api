using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;

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

    public LocationReferenceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
        // Start transaction for consistency
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Resolve Country (check by GeonameId, create snapshot if missing)
            var country = await _unitOfWork.Countries.GetByGeonameIdAsync(countryGeonameId, cancellationToken);
            if (country == null)
            {
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
            }

            // 2. Resolve State (check by GeonameId, create snapshot if missing)
            var state = await _unitOfWork.States.GetByGeonameIdAsync(stateGeonameId, cancellationToken);
            if (state == null)
            {
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
            }

            // 3. Resolve City (check by GeonameId, create snapshot if missing)
            var city = await _unitOfWork.Cities.GetByGeonameIdAsync(cityGeonameId, cancellationToken);
            if (city == null)
            {
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
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Return City ID for foreign key relationships
            return city.Id;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
