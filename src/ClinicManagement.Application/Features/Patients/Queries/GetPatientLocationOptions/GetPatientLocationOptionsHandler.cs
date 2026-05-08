using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientLocationOptionsHandler
    : IRequestHandler<GetPatientLocationOptionsQuery, Result<List<LocationOption>>>
{
    private readonly IPatientRepository _patients;

    public GetPatientLocationOptionsHandler(IPatientRepository patients) => _patients = patients;

    public async Task<Result<List<LocationOption>>> Handle(
        GetPatientLocationOptionsQuery request, CancellationToken ct)
    {
        var options = await _patients.GetLocationOptionsAsync(request.CountryGeonameId, request.StateGeonameId, ct);
        return Result.Success(options);
    }
}
