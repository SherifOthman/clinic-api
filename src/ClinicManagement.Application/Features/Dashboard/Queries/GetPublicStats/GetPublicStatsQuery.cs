using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public record GetPublicStatsQuery : IRequest<Result<PublicStatsDto>>;

/// <summary>Aggregate counts shown on the public marketing website — no auth required.</summary>
public record PublicStatsDto(
    int TotalClinics,
    int TotalPatients,
    int TotalStaff
);
