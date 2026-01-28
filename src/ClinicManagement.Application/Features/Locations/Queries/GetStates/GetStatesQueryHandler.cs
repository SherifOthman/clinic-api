using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetStates;

public class GetStatesQueryHandler : IRequestHandler<GetStatesQuery, Result<List<StateDto>>>
{
    private readonly ILocationsService _locationsService;

    public GetStatesQueryHandler(ILocationsService locationsService)
    {
        _locationsService = locationsService;
    }

    public async Task<Result<List<StateDto>>> Handle(GetStatesQuery request, CancellationToken cancellationToken)
    {
        var states = await _locationsService.GetStatesAsync(request.CountryId);
        return Result<List<StateDto>>.Ok(states);
    }
}
