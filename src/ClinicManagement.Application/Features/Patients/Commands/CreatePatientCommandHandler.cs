using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
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

        // Generate patient code
        var patientCount = await _context.Patients
            .Where(p => p.ClinicId == clinicId)
            .CountAsync(cancellationToken);
        var patientCode = $"P{(patientCount + 1):D6}";

        // Parse blood type
        BloodType? bloodType = null;
        if (!string.IsNullOrEmpty(request.BloodType) && 
            Enum.TryParse<BloodType>(request.BloodType, out var parsedBloodType))
        {
            bloodType = parsedBloodType;
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
                CreatedAt = DateTime.UtcNow
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

        // Return DTO
        var dto = new PatientDto
        {
            Id = patient.Id.ToString(),
            PatientCode = patient.PatientCode,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth.ToString("yyyy-MM-dd"),
            IsMale = patient.IsMale,
            Age = DateTime.UtcNow.Year - patient.DateOfBirth.Year,
            BloodType = patient.BloodType?.ToString(),
            PhoneNumbers = request.PhoneNumbers.Select(p => p.PhoneNumber).ToList(),
            PrimaryPhone = request.PhoneNumbers.FirstOrDefault(p => p.IsPrimary)?.PhoneNumber,
            CreatedAt = patient.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
        };

        return Result<PatientDto>.Success(dto);
    }
}
