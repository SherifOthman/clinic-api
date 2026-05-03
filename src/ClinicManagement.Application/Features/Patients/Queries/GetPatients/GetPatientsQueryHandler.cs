using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
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
        var nationalSearch = !string.IsNullOrWhiteSpace(request.Filter.SearchTerm)
            ? _phoneNormalizer.GetNationalNumber(request.Filter.SearchTerm, _currentUser.CountryCode)
            : null;

        var result = await _uow.Patients.GetProjectedPageAsync(
            request.Filter,
            nationalSearch,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = result.Items.Select(p => new PatientDto
        {
            Id                  = p.Id,
            PatientCode         = p.PatientCode,
            FullName            = p.FullName,
            DateOfBirth         = p.DateOfBirth,
            Gender              = p.Gender,
            BloodType           = p.BloodType,
            ChronicDiseaseCount = p.ChronicDiseaseCount,
            PrimaryPhone        = p.PrimaryPhone,
            CreatedAt           = p.CreatedAt,
            ClinicName          = p.ClinicName,
            CountryGeonameId    = p.CountryGeonameId,
            StateGeonameId      = p.StateGeonameId,
            CityGeonameId       = p.CityGeonameId,
            CityNameEn          = p.CityNameEn,
            CityNameAr          = p.CityNameAr,
        }).ToList();

        return Result.Success(PaginatedResult<PatientDto>.Create(dtos, result.TotalCount, result.PageNumber, result.PageSize));
    }
}
