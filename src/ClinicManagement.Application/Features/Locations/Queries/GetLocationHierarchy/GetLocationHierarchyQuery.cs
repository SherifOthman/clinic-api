using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetLocationHierarchy;

public record GetLocationHierarchyQuery(int GeoNameId) : IRequest<Result<LocationHierarchyDto>>;

public class LocationHierarchyDto
{
    public CountryDto? Country { get; set; }
    public StateDto? State { get; set; }
    public CityDto? City { get; set; }
    public string LocationName { get; set; } = string.Empty;
}