using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetCities;

public record GetCitiesQuery(int StateGeonameId) : IRequest<Result<List<GeoNamesLocationDto>>>;
