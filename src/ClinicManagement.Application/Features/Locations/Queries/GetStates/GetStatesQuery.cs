using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetStates;

public record GetStatesQuery(int CountryGeonameId) : IRequest<Result<List<GeoNamesLocationDto>>>;
