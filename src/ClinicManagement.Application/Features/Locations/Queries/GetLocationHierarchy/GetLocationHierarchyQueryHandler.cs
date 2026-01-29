using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Locations.Queries.GetLocationHierarchy;

public class GetLocationHierarchyQueryHandler : IRequestHandler<GetLocationHierarchyQuery, Result<LocationHierarchyDto>>
{
    private readonly IGeoNamesClient _geoNamesClient;
    private readonly ILocationsService _locationsService;
    private readonly ILogger<GetLocationHierarchyQueryHandler> _logger;

    public GetLocationHierarchyQueryHandler(
        IGeoNamesClient geoNamesClient,
        ILocationsService locationsService,
        ILogger<GetLocationHierarchyQueryHandler> logger)
    {
        _geoNamesClient = geoNamesClient;
        _locationsService = locationsService;
        _logger = logger;
    }

    public async Task<Result<LocationHierarchyDto>> Handle(GetLocationHierarchyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the location details from GeoNames API
            var locationDetails = await _geoNamesClient.GetLocationByIdAsync(request.GeoNameId);
            if (locationDetails == null)
            {
                return Result<LocationHierarchyDto>.Fail(MessageCodes.Location.NOT_FOUND);
            }

            var hierarchy = new LocationHierarchyDto
            {
                LocationName = locationDetails.Name
            };

            // Get country information
            if (!string.IsNullOrEmpty(locationDetails.CountryCode))
            {
                var country = await _locationsService.GetCountryByCodeAsync(locationDetails.CountryCode);
                if (country != null)
                {
                    hierarchy.Country = country;

                    // If we have admin names, try to find state and city
                    if (!string.IsNullOrEmpty(locationDetails.AdminName1))
                    {
                        var states = await _locationsService.GetStatesAsync(country.Id);
                        var state = states.FirstOrDefault(s => 
                            s.Name.Equals(locationDetails.AdminName1, StringComparison.OrdinalIgnoreCase));
                        
                        if (state != null)
                        {
                            hierarchy.State = state;

                            // Try to find the city
                            var cities = await _locationsService.GetCitiesAsync(country.Id, state.Id);
                            var city = cities.FirstOrDefault(c => c.Id == request.GeoNameId);
                            
                            if (city != null)
                            {
                                hierarchy.City = city;
                            }
                        }
                    }
                }
            }

            return Result<LocationHierarchyDto>.Ok(hierarchy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get location hierarchy for GeoNameId {GeoNameId}", request.GeoNameId);
            return Result<LocationHierarchyDto>.Fail(MessageCodes.Location.HIERARCHY_FAILED);
        }
    }
}