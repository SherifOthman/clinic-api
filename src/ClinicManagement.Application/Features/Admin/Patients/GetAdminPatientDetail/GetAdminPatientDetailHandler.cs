using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Patients;

public class GetAdminPatientDetailHandler : IRequestHandler<GetAdminPatientDetailQuery, Result<PatientDetailDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAdminPatientDetailHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PatientDetailDto>> Handle(
        GetAdminPatientDetailQuery request, CancellationToken cancellationToken)
    {
        var data = await _uow.Patients.GetAdminDetailAsync(request.PatientId, cancellationToken);

        if (data is null)
            return Result.Failure<PatientDetailDto>(ErrorCodes.PATIENT_NOT_FOUND, "Patient not found");

        return Result.Success(new PatientDetailDto
        {
            Id               = data.Id.ToString(),
            PatientCode      = data.PatientCode,
            FullName         = data.FullName,
            DateOfBirth      = data.DateOfBirth,
            Gender           = data.Gender,
            BloodType        = data.BloodType,
            CountryGeonameId = data.CountryGeonameId,
            StateGeonameId   = data.StateGeonameId,
            CityGeonameId    = data.CityGeonameId,
            CountryNameEn    = data.CountryNameEn,
            CountryNameAr    = data.CountryNameAr,
            StateNameEn      = data.StateNameEn,
            StateNameAr      = data.StateNameAr,
            CityNameEn       = data.CityNameEn,
            CityNameAr       = data.CityNameAr,
            PhoneNumbers     = data.Phones,
            ChronicDiseases  = data.Diseases.Select(d => new PatientChronicDiseaseDto(d.Id, d.NameEn, d.NameAr)).ToList(),
            CreatedAt        = data.CreatedAt,
            UpdatedAt        = data.UpdatedAt,
            CreatedBy        = data.CreatedBy.HasValue && data.AuditUserNames.TryGetValue(data.CreatedBy.Value, out var cb) ? cb : null,
            UpdatedBy        = data.UpdatedBy.HasValue && data.AuditUserNames.TryGetValue(data.UpdatedBy.Value, out var ub) ? ub : null,
            ClinicId         = data.ClinicId.ToString(),
            ClinicName       = data.ClinicName,
        });
    }
}
