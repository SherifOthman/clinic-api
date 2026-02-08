using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class LocationReferenceService : ILocationReferenceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LocationReferenceService> _logger;

    public LocationReferenceService(IUnitOfWork unitOfWork, ILogger<LocationReferenceService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ResolveLocationAsync(
        int countryGeonameId,
        CountrySnapshotData countryData,
        int stateGeonameId,
        StateSnapshotData stateData,
        int cityGeonameId,
        CitySnapshotData cityData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating location snapshots - Country: {CountryId}, State: {StateId}, City: {CityId}",
            countryGeonameId, stateGeonameId, cityGeonameId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await EnsureLocationSnapshotAsync(countryGeonameId, LocationType.Country, countryData.NameEn, countryData.NameAr, cancellationToken);
            await EnsureLocationSnapshotAsync(stateGeonameId, LocationType.State, stateData.NameEn, stateData.NameAr, cancellationToken);
            await EnsureLocationSnapshotAsync(cityGeonameId, LocationType.City, cityData.NameEn, cityData.NameAr, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Location snapshots created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create location snapshots");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task EnsureLocationSnapshotAsync(
        int geoNameId,
        LocationType type,
        string nameEn,
        string nameAr,
        CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.LocationSnapshots.GetByGeoNameIdAsync(geoNameId, cancellationToken);

        if (existing == null)
        {
            var snapshot = new LocationSnapshot
            {
                GeoNameId = geoNameId,
                Type = type,
                NameEn = nameEn,
                NameAr = nameAr,
                Provider = "GeoNames",
                LastSyncedAt = DateTime.UtcNow
            };

            await _unitOfWork.LocationSnapshots.AddAsync(snapshot);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created location snapshot - GeoNameId: {GeoNameId}, Type: {Type}", geoNameId, type);
        }
    }
}
