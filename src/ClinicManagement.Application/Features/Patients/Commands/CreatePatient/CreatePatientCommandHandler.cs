using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using System.Globalization;
using static ClinicManagement.Domain.Enums.BloodTypeExtensions;

namespace ClinicManagement.Application.Features.Patients.Commands;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Result<Guid>>
{
    private readonly IPatientRepository      _patients;
    private readonly IPatientCounterRepository _patientCounters;
    private readonly IUnitOfWork             _uow;
    private readonly ICurrentUserService     _currentUser;
    private readonly IPhoneNormalizer        _phoneNormalizer;

    public CreatePatientCommandHandler(
        IPatientRepository patients,
        IPatientCounterRepository patientCounters,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IPhoneNormalizer phoneNormalizer)
    {
        _patients        = patients;
        _patientCounters = patientCounters;
        _uow             = uow;
        _currentUser     = currentUser;
        _phoneNormalizer = phoneNormalizer;
    }

    public async Task<Result<Guid>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var clinicId    = _currentUser.GetRequiredClinicId();
        var patientCode = await _patientCounters.NextCodeAsync(clinicId, cancellationToken);
        var gender      = Enum.TryParse<Domain.Enums.Gender>(request.Gender, out var pg) ? pg : Domain.Enums.Gender.Male;
        // InvariantCulture: validator enforces YYYY-MM-DD format
        var dob = DateOnly.Parse(request.DateOfBirth, CultureInfo.InvariantCulture);

        var patient = new Patient
        {
            ClinicId         = clinicId,
            PatientCode      = patientCode,
            FullName         = request.FullName.Trim(),
            Gender           = gender,
            DateOfBirth      = dob,
            CountryGeonameId = request.CountryGeonameId,
            StateGeonameId   = request.StateGeonameId,
            CityGeonameId    = request.CityGeonameId,
            BloodType        = ParseBloodType(request.BloodType),
            CreatedAt        = DateTimeOffset.UtcNow,
        };

        await _patients.AddAsync(patient);

        PatientPhoneHelper.ReplacePhones(
            _patients, _phoneNormalizer,
            patient.Id, request.PhoneNumbers,
            _currentUser.CountryCode);

        foreach (var diseaseId in request.ChronicDiseaseIds)
            _patients.AddChronicDisease(new PatientChronicDisease { PatientId = patient.Id, ChronicDiseaseId = diseaseId });

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success(patient.Id);
    }
}
