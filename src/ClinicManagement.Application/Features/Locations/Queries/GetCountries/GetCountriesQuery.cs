using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetCountries;

public record GetCountriesQuery : IRequest<Result<List<CountryDto>>>;
