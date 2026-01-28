using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetGeoNamesHealth;

public record GetGeoNamesHealthQuery : IRequest<Result<GeoNamesHealthDto>>;
