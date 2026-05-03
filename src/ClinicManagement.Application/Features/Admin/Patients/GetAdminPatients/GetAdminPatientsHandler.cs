using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Patients;

public class GetAdminPatientsHandler : IRequestHandler<GetAdminPatientsQuery, Result<PaginatedResult<PatientDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IPhoneNormalizer _phoneNormalizer;

    public GetAdminPatientsHandler(IUnitOfWork uow, IPhoneNormalizer phoneNormalizer)
    {
        _uow             = uow;
        _phoneNormalizer = phoneNormalizer;
    }

    public async Task<Result<PaginatedResult<PatientDto>>> Handle(
        GetAdminPatientsQuery request, CancellationToken cancellationToken)
    {
        // No country code for SuperAdmin — phone normalization skipped
        var nationalSearch = !string.IsNullOrWhiteSpace(request.Filter.SearchTerm)
            ? _phoneNormalizer.GetNationalNumber(request.Filter.SearchTerm, null)
            : null;

        var result = await _uow.Patients.GetAdminProjectedPageAsync(
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
