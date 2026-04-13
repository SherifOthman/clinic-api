using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetDistinctPatientCountryIdsHandler : IRequestHandler<GetDistinctPatientCountryIdsQuery, Result<List<int>>>
{
    private readonly IUnitOfWork _uow;
    public GetDistinctPatientCountryIdsHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<Result<List<int>>> Handle(GetDistinctPatientCountryIdsQuery request, CancellationToken ct)
        => Result.Success(await _uow.Patients.GetDistinctCountryGeonameIdsAsync(request.IsSuperAdmin, ct));
}

public class GetDistinctPatientStateIdsHandler : IRequestHandler<GetDistinctPatientStateIdsQuery, Result<List<int>>>
{
    private readonly IUnitOfWork _uow;
    public GetDistinctPatientStateIdsHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<Result<List<int>>> Handle(GetDistinctPatientStateIdsQuery request, CancellationToken ct)
        => Result.Success(await _uow.Patients.GetDistinctStateGeonameIdsAsync(request.IsSuperAdmin, ct));
}

public class GetDistinctPatientCityIdsHandler : IRequestHandler<GetDistinctPatientCityIdsQuery, Result<List<int>>>
{
    private readonly IUnitOfWork _uow;
    public GetDistinctPatientCityIdsHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<Result<List<int>>> Handle(GetDistinctPatientCityIdsQuery request, CancellationToken ct)
        => Result.Success(await _uow.Patients.GetDistinctCityGeonameIdsAsync(request.IsSuperAdmin, ct));
}
