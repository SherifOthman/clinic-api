using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        // Generate unpredictable patient code: P + 3 letters + 3 digits (e.g. PXKM847)
        // Retry on collision (extremely rare)
        string patientCode;
        do
        {
            patientCode = GeneratePatientCode();
        }
        // Global uniqueness — patient code is a system-wide identifier (patient portal, cross-clinic)
        while (await _context.Patients
            .IgnoreQueryFilters()
            .AnyAsync(p => p.PatientCode == patientCode, cancellationToken));

        // Parse blood type — frontend sends display string (A+, B-, etc.)
        BloodType? bloodType = null;
        if (!string.IsNullOrEmpty(request.BloodType))
        {
            bloodType = request.BloodType switch
            {
                "A+"  => BloodType.APositive,
                "A-"  => BloodType.ANegative,
                "B+"  => BloodType.BPositive,
                "B-"  => BloodType.BNegative,
                "AB+" => BloodType.ABPositive,
                "AB-" => BloodType.ABNegative,
                "O+"  => BloodType.OPositive,
                "O-"  => BloodType.ONegative,
                _     => null,
            };
        }

        // Create patient
        var patient = new Patient
        {
            ClinicId = clinicId,
            PatientCode = patientCode,
            FullName = request.FullName,
            DateOfBirth = DateTime.Parse(request.DateOfBirth),
            IsMale = request.Gender == "Male",
            CityGeoNameId = request.CityGeoNameId,
            BloodType = bloodType,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            EmergencyContactRelation = request.EmergencyContactRelation,
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

    private static string GeneratePatientCode()
    {
        // 8 random digits — 90 million combinations, globally unique across all clinics
        // Short enough to read/type, unpredictable, works for patient portal
        return Random.Shared.Next(10000000, 99999999).ToString();
    }
}
