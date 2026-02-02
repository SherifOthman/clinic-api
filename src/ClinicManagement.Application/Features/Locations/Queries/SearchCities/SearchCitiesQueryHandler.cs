using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.SearchCities;

public class SearchCitiesQueryHandler : IRequestHandler<SearchCitiesQuery, Result<List<CityDto>>>
{
    private readonly ILocationsService _locationsService;

    public SearchCitiesQueryHandler(ILocationsService locationsService)
    {
        _locationsService = locationsService;
    }

    public async Task<Result<List<CityDto>>> Handle(SearchCitiesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 2)
        {
            return Result<List<CityDto>>.Fail(MessageCodes.Validation.FIELD_INVALID_LENGTH);
        }

        var cities = await _locationsService.SearchCitiesAsync(request.CountryCode, request.Query);
        return Result<List<CityDto>>.Ok(cities);
    }
}
