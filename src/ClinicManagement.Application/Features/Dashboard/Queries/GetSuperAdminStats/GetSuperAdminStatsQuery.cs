using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public record GetSuperAdminStatsQuery : IRequest<Result<SuperAdminStatsDto>>;

public record SuperAdminStatsDto(
    int TotalClinics,
    int TotalPatients,
    int TotalStaff,
    int ClinicsOnTrial,
    int ClinicsActive
);
