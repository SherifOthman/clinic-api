using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using static ClinicManagement.Domain.Enums.BloodTypeExtensions;

namespace ClinicManagement.Application.Features.Patients.Commands;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IPhoneNormalizer _phoneNormalizer;

    public CreatePatientCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, IPhoneNormalizer phoneNormalizer)
    {
        _uow             = uow;
        _currentUser     = currentUser;
        _phoneNormalizer = phoneNormalizer;
    }

    public async Task<Result<Guid>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var clinicId    = _currentUser.GetRequiredClinicId();
        var patientCode = await _uow.PatientCounters.NextCodeAsync(clinicId, cancellationToken);
        var gender      = Enum.TryParse<Domain.Enums.Gender>(request.Gender, out var pg) ? pg : Domain.Enums.Gender.Male;
        var dob         = DateOnly.Parse(request.DateOfBirth);

        var person = new Person
        {
            FirstName   = request.FirstName.Trim(),
            LastName    = request.LastName.Trim(),
            Gender      = gender,
            DateOfBirth = dob,
        };

        var patient = new Patient
        {
            ClinicId         = clinicId,
            PatientCode      = patientCode,
            CountryGeonameId = request.CountryGeonameId,
            StateGeonameId   = request.StateGeonameId,
            CityGeonameId    = request.CityGeonameId,
            BloodType        = ParseBloodType(request.BloodType),
            CreatedAt        = DateTimeOffset.UtcNow,
            PersonId         = person.Id,
            Person           = person,
        };

        await _uow.Patients.AddAsync(patient);

        var countryCode = _currentUser.CountryCode;

        foreach (var phone in request.PhoneNumbers)
            _uow.Patients.AddPhone(new PatientPhone
            {
                PatientId      = patient.Id,
                PhoneNumber    = phone,
                NationalNumber = _phoneNormalizer.GetNationalNumber(phone, countryCode) ?? phone,
            });

        foreach (var diseaseId in request.ChronicDiseaseIds)
            _uow.Patients.AddChronicDisease(new PatientChronicDisease { PatientId = patient.Id, ChronicDiseaseId = diseaseId });

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success(patient.Id);
    }
}
