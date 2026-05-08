using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public class GetRecentPatientsHandler : IRequestHandler<GetRecentPatientsQuery, Result<List<RecentPatientDto>>>
{
    private readonly IPatientRepository _patients;

    public GetRecentPatientsHandler(IPatientRepository patients) => _patients = patients;

    public async Task<Result<List<RecentPatientDto>>> Handle(
        GetRecentPatientsQuery request, CancellationToken cancellationToken)
    {
        var rows = await _patients.GetRecentAsync(request.Count, cancellationToken);
        return Result.Success(rows.Select(r => new RecentPatientDto(
            r.Id, r.PatientCode, r.FullName, r.DateOfBirth, r.Gender, r.CreatedAt
        )).ToList());
    }
}
