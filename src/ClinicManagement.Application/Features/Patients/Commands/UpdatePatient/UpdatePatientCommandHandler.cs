using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using static ClinicManagement.Domain.Enums.BloodTypeExtensions;

namespace ClinicManagement.Application.Features.Patients.Commands;

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IPhoneNormalizer _phoneNormalizer;

    public UpdatePatientCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, IPhoneNormalizer phoneNormalizer)
    {
        _uow             = uow;
        _currentUser     = currentUser;
        _phoneNormalizer = phoneNormalizer;
    }

    public async Task<Result> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _uow.Patients.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (patient is null)
            return Result.Failure(ErrorCodes.PATIENT_NOT_FOUND, "Patient not found");

        patient.FullName          = request.FullName;
        patient.DateOfBirth       = DateOnly.Parse(request.DateOfBirth);
        patient.Gender            = Enum.TryParse<Domain.Enums.Gender>(request.Gender, out var ug) ? ug : Domain.Enums.Gender.Male;
        patient.CountryGeonameId  = request.CountryGeonameId;
        patient.StateGeonameId    = request.StateGeonameId;
        patient.CityGeonameId     = request.CityGeonameId;
        patient.BloodType         = ParseBloodType(request.BloodType);

        if (request.PhoneNumbers != null)
        {
            foreach (var phone in patient.Phones?.ToList() ?? [])
                _uow.Patients.RemovePhone(phone);

            // Read from JWT — no DB call
            var countryCode = _currentUser.CountryCode;

            foreach (var phone in request.PhoneNumbers)
                _uow.Patients.AddPhone(new PatientPhone
                {
                    PatientId      = patient.Id,
                    PhoneNumber    = phone,
                    NationalNumber = _phoneNormalizer.GetNationalNumber(phone, countryCode) ?? phone,
                });
        }

        if (request.ChronicDiseaseIds != null)
        {
            foreach (var cd in patient.ChronicDiseases?.ToList() ?? [])
                _uow.Patients.RemoveChronicDisease(cd);

            foreach (var diseaseId in request.ChronicDiseaseIds)
                _uow.Patients.AddChronicDisease(new PatientChronicDisease { PatientId = patient.Id, ChronicDiseaseId = diseaseId });
        }

        patient.Touch();
        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
