using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Patients;

public class GetAdminPatientLocationOptionsHandler
    : IRequestHandler<GetAdminPatientLocationOptionsQuery, Result<List<LocationOption>>>
{
    private readonly IUnitOfWork _uow;

    public GetAdminPatientLocationOptionsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<LocationOption>>> Handle(
        GetAdminPatientLocationOptionsQuery request, CancellationToken ct)
    {
        var options = await _uow.Patients.GetAdminLocationOptionsAsync(
            request.CountryGeonameId,
            request.StateGeonameId,
            ct);

        return Result.Success(options);
    }
}
