using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.SearchCities;

public record SearchCitiesQuery(string CountryCode, string Query) : IRequest<Result<List<CityDto>>>;
