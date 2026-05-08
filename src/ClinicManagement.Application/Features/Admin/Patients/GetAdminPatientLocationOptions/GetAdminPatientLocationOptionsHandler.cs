using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Patients;

public class GetAdminPatientLocationOptionsHandler
    : IRequestHandler<GetAdminPatientLocationOptionsQuery, Result<List<LocationOption>>>
{
    private readonly IPatientRepository _patients;

    public GetAdminPatientLocationOptionsHandler(IPatientRepository patients) => _patients = patients;

    public async Task<Result<List<LocationOption>>> Handle(
        GetAdminPatientLocationOptionsQuery request, CancellationToken ct)
    {
        var options = await _patients.GetAdminLocationOptionsAsync(request.CountryGeonameId, request.StateGeonameId, ct);
        return Result.Success(options);
    }
}
