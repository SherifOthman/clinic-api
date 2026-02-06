using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Implements lazy seeding for GeoNames location data
/// Architecture: Snapshot + Lazy Seeding
/// - GeoNames is external provider only (not primary data source)
/// - Check by GeonameId, insert snapshot if not exists
/// - Wrap in transaction for consistency
/// - Do NOT auto-update existing snapshots
/// </summary>
public class LocationSeedingService : ILocationSeedingService
{
    private readonly IUnitOfWork _unitOfWork;

    public LocationSeedingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> EnsureLocationExistsAsync(
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
            // 1. Ensure Country exists (check by GeonameId)
            var country = await _unitOfWork.Countries.GetByGeonameIdAsync(countryGeonameId, cancellationToken);
            if (country == null)
            {
                // Insert snapshot from GeoNames data
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

            // 2. Ensure State exists (check by GeonameId)
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

            // 3. Ensure City exists (check by GeonameId)
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
