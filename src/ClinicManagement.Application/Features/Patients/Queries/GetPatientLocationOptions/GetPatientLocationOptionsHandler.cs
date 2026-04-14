using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientLocationOptionsHandler
    : IRequestHandler<GetPatientLocationOptionsQuery, Result<List<LocationOption>>>
{
    private readonly IUnitOfWork _uow;

    public GetPatientLocationOptionsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<LocationOption>>> Handle(
        GetPatientLocationOptionsQuery request, CancellationToken ct)
    {
        var options = await _uow.Patients.GetLocationOptionsAsync(
            request.CountryGeonameId,
            request.StateGeonameId,
            request.IsSuperAdmin,
            ct);

        return Result.Success(options);
    }
}
