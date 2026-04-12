using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientStatesQueryHandler : IRequestHandler<GetPatientStatesQuery, Result<List<PatientStateDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetPatientStatesQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<PatientStateDto>>> Handle(
        GetPatientStatesQuery request, CancellationToken cancellationToken)
    {
        var states = await _uow.Patients.GetDistinctStatesAsync(request.IsSuperAdmin, cancellationToken);
        return Result.Success(states);
    }
}
