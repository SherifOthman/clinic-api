using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatients;

public record GetPatientsQuery : IRequest<Result<PaginatedList<PatientDto>>>
{
    public int? ClinicId { get; set; }
    public string? SearchTerm { get; set; }
    public Gender? Gender { get; set; }
    public string? City { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
