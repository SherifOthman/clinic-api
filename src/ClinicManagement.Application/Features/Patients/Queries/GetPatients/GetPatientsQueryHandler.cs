using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Patients.QueryModels;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, Result<PaginatedResult<PatientDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IPhoneNormalizer _phoneNormalizer;

    public GetPatientsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser, IPhoneNormalizer phoneNormalizer)
    {
        _uow             = uow;
        _currentUser     = currentUser;
        _phoneNormalizer = phoneNormalizer;
    }

    public async Task<Result<PaginatedResult<PatientDto>>> Handle(
        GetPatientsQuery request, CancellationToken cancellationToken)
    {
        // Used to strip the correct trunk prefix from the search term so
        // "010" → "10" for Egypt, "07" → "7" for UK, etc.
        var countryCode = _currentUser.CountryCode;

        var nationalSearch = !string.IsNullOrWhiteSpace(request.SearchTerm)
            ? _phoneNormalizer.GetNationalNumber(request.SearchTerm, countryCode)
            : null;

        var result = await _uow.Patients.GetProjectedPageAsync(
            request.SearchTerm,
            nationalSearch,
            request.Gender,
            request.SortBy,
            request.SortDirection,
            request.ClinicSearch,
            request.StateGeonameId,
            request.CityGeonameId,
            request.CountryGeonameId,
            request.IsSuperAdmin,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = result.Items.Adapt<List<PatientDto>>();

        return Result.Success(PaginatedResult<PatientDto>.Create(dtos, result.TotalCount, result.PageNumber, result.PageSize));
    }
}
