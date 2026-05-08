using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientDetailHandler : IRequestHandler<GetPatientDetailQuery, Result<PatientDetailDto>>
{
    private readonly IPatientRepository _patients;

    public GetPatientDetailHandler(IPatientRepository patients) => _patients = patients;

    public async Task<Result<PatientDetailDto>> Handle(
        GetPatientDetailQuery request, CancellationToken cancellationToken)
    {
        var data = await _patients.GetDetailAsync(request.PatientId, cancellationToken);

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
            CreatedBy        = data.CreatedBy,
            UpdatedBy        = data.UpdatedBy,
            ClinicId         = null,
            ClinicName       = null,
        });
    }
}
