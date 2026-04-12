using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientCountriesQueryHandler : IRequestHandler<GetPatientCountriesQuery, Result<List<PatientStateDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetPatientCountriesQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<PatientStateDto>>> Handle(
        GetPatientCountriesQuery request, CancellationToken cancellationToken)
    {
        var countries = await _uow.Patients.GetDistinctCountriesAsync(request.IsSuperAdmin, cancellationToken);
        return Result.Success(countries);
    }
}
