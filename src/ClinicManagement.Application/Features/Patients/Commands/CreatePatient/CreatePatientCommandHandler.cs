using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static ClinicManagement.Domain.Enums.BloodTypeExtensions;

namespace ClinicManagement.Application.Features.Patients.Commands;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Result<PatientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreatePatientCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PatientDto>> Handle(
        CreatePatientCommand request,
        CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        var patientCode = await GenerateUniquePatientCodeAsync(cancellationToken);

        // Parse blood type — frontend sends display string (A+, B-, etc.)
        var bloodType = ParseBloodType(request.BloodType);        // Create patient
        var patient = new Patient
        {
            ClinicId = clinicId,
            PatientCode = patientCode,
            FullName = request.FullName,
            DateOfBirth = DateTime.Parse(request.DateOfBirth),
            IsMale = request.Gender == "Male",
            CountryGeoNameId = request.CountryGeoNameId,
            StateGeoNameId = request.StateGeoNameId,
            CityGeoNameId = request.CityGeoNameId,
            BloodType = bloodType,
            CreatedAt = DateTime.UtcNow
        };

        _context.Patients.Add(patient);

        // Add phone numbers
        foreach (var phone in request.PhoneNumbers)
        {
            var patientPhone = new PatientPhone
            {
                PatientId = patient.Id,
                PhoneNumber = phone.PhoneNumber,
                IsPrimary = phone.IsPrimary,
            };
            _context.PatientPhones.Add(patientPhone);
        }

        // Add chronic diseases
        foreach (var diseaseId in request.ChronicDiseaseIds)
        {
            var patientDisease = new PatientChronicDisease
            {
                PatientId = patient.Id,
                ChronicDiseaseId = diseaseId
            };
            _context.PatientChronicDiseases.Add(patientDisease);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = patient.Adapt<PatientDto>();
        return Result<PatientDto>.Success(dto);
    }


    private async Task<string> GenerateUniquePatientCodeAsync(CancellationToken ct)
    {
        // Retry on collision (extremely rare)
        string patientCode;
        do
        {
            patientCode = GeneratePatientCode();
        }
        // Global uniqueness — patient code is a system-wide identifier (patient portal, cross-clinic)
        while (await _context.Patients
            .IgnoreQueryFilters()
            .AnyAsync(p => p.PatientCode == patientCode, ct));

        return patientCode;
    }

    internal static string GeneratePatientCode()
    {
        // 8 random digits — 90 million combinations, globally unique across all clinics
        // Short enough to read/type, unpredictable, works for patient portal
        return Random.Shared.Next(10000000, 99999999).ToString();
    }
}
