using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// On-demand location reference resolver for GeoNames snapshot architecture.
/// 
/// CRITICAL DESIGN: This is NOT database seeding or startup initialization.
/// Location snapshots are created lazily during user operations (registration, onboarding, profile updates).
/// 
/// Architecture:
/// - Client fetches location data from GeoNames API
/// - Service creates local snapshots only if missing (idempotent)
/// - Returns CityId for foreign key relationships
/// - Transaction-safe, no runtime GeoNames dependency
/// - Never runs at startup, migrations, or as background job
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

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var country = await _unitOfWork.Countries.GetByGeonameIdAsync(countryGeonameId, cancellationToken);
            if (country == null)
            {
                _logger.LogInformation("Country not found in local database, creating snapshot - GeonameId: {GeonameId}, Name: {NameEn}",
                    countryGeonameId, countryData.NameEn);

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

            var state = await _unitOfWork.States.GetByGeonameIdAsync(stateGeonameId, cancellationToken);
            if (state == null)
            {
                _logger.LogInformation("State not found in local database, creating snapshot - GeonameId: {GeonameId}, Name: {NameEn}",
                    stateGeonameId, stateData.NameEn);

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

            var city = await _unitOfWork.Cities.GetByGeonameIdAsync(cityGeonameId, cancellationToken);
            if (city == null)
            {
                _logger.LogInformation("City not found in local database, creating snapshot - GeonameId: {GeonameId}, Name: {NameEn}",
                    cityGeonameId, cityData.NameEn);

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
