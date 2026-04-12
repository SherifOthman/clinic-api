using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientCitiesQueryHandler : IRequestHandler<GetPatientCitiesQuery, Result<List<PatientStateDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetPatientCitiesQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<PatientStateDto>>> Handle(
        GetPatientCitiesQuery request, CancellationToken cancellationToken)
    {
        var cities = await _uow.Patients.GetDistinctCitiesAsync(request.IsSuperAdmin, cancellationToken);
        return Result.Success(cities);
    }
}
