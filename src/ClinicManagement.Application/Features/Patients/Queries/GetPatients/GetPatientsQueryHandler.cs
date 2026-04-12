using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Patients.QueryModels;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, Result<PaginatedResult<PatientDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetPatientsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedResult<PatientDto>>> Handle(
        GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var result = await _uow.Patients.GetProjectedPageAsync(
            request.SearchTerm,
            request.Gender,
            request.SortBy,
            request.SortDirection,
            request.ClinicSearch,
            request.StateSearch,
            request.CitySearch,
            request.CountrySearch,
            request.IsSuperAdmin,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = result.Items.Adapt<List<PatientDto>>();

        return Result.Success(PaginatedResult<PatientDto>.Create(dtos, result.TotalCount, result.PageNumber, result.PageSize));
    }
}
