using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands;

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, Result<PatientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdatePatientCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PatientDto>> Handle(
        UpdatePatientCommand request,
        CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        var patient = await _context.Patients
            .Include(p => p.Phones)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.ClinicId == clinicId && !p.IsDeleted, cancellationToken);

        if (patient is null)
        {
            return Result.Failure<PatientDto>("PATIENT.NOT_FOUND", "Patient not found");
        }

        BloodType? bloodType = null;
        if (!string.IsNullOrEmpty(request.BloodType) && 
            Enum.TryParse<BloodType>(request.BloodType, out var parsedBloodType))
        {
            bloodType = parsedBloodType;
        }

        patient.FullName = request.FullName;
        patient.DateOfBirth = DateTime.Parse(request.DateOfBirth);
        patient.IsMale = request.Gender == "Male";
        patient.CityGeoNameId = request.CityGeoNameId;
        patient.BloodType = bloodType;
        patient.EmergencyContactName = request.EmergencyContactName;
        patient.EmergencyContactPhone = request.EmergencyContactPhone;
        patient.EmergencyContactRelation = request.EmergencyContactRelation;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new PatientDto
        {
            Id = patient.Id.ToString(),
            PatientCode = patient.PatientCode,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth.ToString("yyyy-MM-dd"),
            IsMale = patient.IsMale,
            Age = DateTime.UtcNow.Year - patient.DateOfBirth.Year,
            BloodType = patient.BloodType?.ToString(),
            PhoneNumbers = patient.Phones.Select(p => p.PhoneNumber).ToList(),
            PrimaryPhone = patient.Phones.FirstOrDefault(p => p.IsPrimary)?.PhoneNumber,
            CreatedAt = patient.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
        };

        return Result.Success(dto);
    }
}
