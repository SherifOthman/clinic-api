using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientDetailHandler : IRequestHandler<GetPatientDetailQuery, Result<PatientDetailDto>>
{
    private readonly IUnitOfWork _uow;

    public GetPatientDetailHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PatientDetailDto>> Handle(
        GetPatientDetailQuery request, CancellationToken cancellationToken)
    {
        var data = await _uow.Patients.GetDetailAsync(request.PatientId, request.IsSuperAdmin, cancellationToken);

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
            CityNameEn       = data.CityNameEn,
            CityNameAr       = data.CityNameAr,
            StateNameEn      = data.StateNameEn,
            StateNameAr      = data.StateNameAr,
            CountryNameEn    = data.CountryNameEn,
            CountryNameAr    = data.CountryNameAr,
            PhoneNumbers     = data.Phones,
            ChronicDiseases  = data.Diseases.Select(d => new PatientChronicDiseaseDto(d.Id, d.NameEn, d.NameAr)).ToList(),
            CreatedAt        = data.CreatedAt,
            UpdatedAt        = data.UpdatedAt,
            CreatedBy        = data.CreatedBy.HasValue && data.AuditUserNames.TryGetValue(data.CreatedBy.Value, out var cb) ? cb : null,
            UpdatedBy        = data.UpdatedBy.HasValue && data.AuditUserNames.TryGetValue(data.UpdatedBy.Value, out var ub) ? ub : null,
            ClinicId         = request.IsSuperAdmin ? data.ClinicId.ToString() : null,
            ClinicName       = data.ClinicName,
        });
    }
}
