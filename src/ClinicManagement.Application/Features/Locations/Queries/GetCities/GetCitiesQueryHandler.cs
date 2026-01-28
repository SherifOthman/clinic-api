using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetCities;

public class GetCitiesQueryHandler : IRequestHandler<GetCitiesQuery, Result<List<CityDto>>>
{
    private readonly ILocationsService _locationsService;

    public GetCitiesQueryHandler(ILocationsService locationsService)
    {
        _locationsService = locationsService;
    }

    public async Task<Result<List<CityDto>>> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
    {
        var cities = await _locationsService.GetCitiesAsync(request.CountryId, request.StateId);
        return Result<List<CityDto>>.Ok(cities);
    }
}
