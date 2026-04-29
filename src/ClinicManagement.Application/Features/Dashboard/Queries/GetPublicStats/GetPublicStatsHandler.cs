using ClinicManagement.Application.Abstractions.Data;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public class GetPublicStatsHandler : IRequestHandler<GetPublicStatsQuery, PublicStatsDto>
{
    private readonly IUnitOfWork _uow;

    public GetPublicStatsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PublicStatsDto> Handle(GetPublicStatsQuery request, CancellationToken ct)
    {
        var clinics  = await _uow.Clinics.CountIgnoreFiltersAsync(ct);
        var patients = await _uow.Patients.CountIgnoreFiltersAsync(ct);
        var staff    = await _uow.Members.CountActiveIgnoreFiltersAsync(ct);

        return new PublicStatsDto(clinics, patients, staff);
    }
}
