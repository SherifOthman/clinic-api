using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public record GetRecentPatientsQuery(int Count = 5) : IRequest<Result<List<RecentPatientDto>>>;

public record RecentPatientDto(
    string Id,
    string PatientCode,
    string FullName,
    DateOnly DateOfBirth,
    string Gender,
    DateTimeOffset RegisteredAt
);
