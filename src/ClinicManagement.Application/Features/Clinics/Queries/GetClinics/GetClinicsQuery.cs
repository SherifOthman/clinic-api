using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Clinics.Queries.GetClinics;

public record GetClinicsQuery : IRequest<Result<List<ClinicDto>>>
{
    public int? OwnerId { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
