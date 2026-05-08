using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public class GetPublicStatsHandler : IRequestHandler<GetPublicStatsQuery, Result<PublicStatsDto>>
{
    private readonly IClinicRepository       _clinics;
    private readonly IPatientRepository      _patients;
    private readonly IClinicMemberRepository _members;

    public GetPublicStatsHandler(
        IClinicRepository clinics,
        IPatientRepository patients,
        IClinicMemberRepository members)
    {
        _clinics  = clinics;
        _patients = patients;
        _members  = members;
    }

    public async Task<Result<PublicStatsDto>> Handle(GetPublicStatsQuery request, CancellationToken ct)
    {
        var clinics  = await _clinics.CountIgnoreFiltersAsync(ct);
        var patients = await _patients.CountIgnoreFiltersAsync(ct);
        var staff    = await _members.CountActiveIgnoreFiltersAsync(ct);

        return Result.Success(new PublicStatsDto(clinics, patients, staff));
    }
}
