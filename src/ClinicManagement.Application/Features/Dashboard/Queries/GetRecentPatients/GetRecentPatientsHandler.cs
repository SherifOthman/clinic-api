using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public class GetRecentPatientsHandler : IRequestHandler<GetRecentPatientsQuery, Result<List<RecentPatientDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetRecentPatientsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<RecentPatientDto>>> Handle(
        GetRecentPatientsQuery request, CancellationToken cancellationToken)
    {
        var rows = await _uow.Patients.GetRecentAsync(request.Count, cancellationToken);
        return Result.Success(rows.Adapt<List<RecentPatientDto>>());
    }
}
