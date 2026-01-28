using ClinicManagement.Application.DTOs;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IGeoNamesClient
{
    Task<List<GeoNamesLocationDto>> SearchAsync(GeoNamesSearchRequest request, CancellationToken cancellationToken = default);
    Task<GeoNamesLocationDto?> GetLocationByIdAsync(int geoNameId, CancellationToken cancellationToken = default);
}
