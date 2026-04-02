using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using Mapster;
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
        // ClinicId filter applied automatically via global named filter
        var patient = await _context.Patients
            .Include(p => p.Phones)
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (patient is null)
        {
            return Result.Failure<PatientDto>(ErrorCodes.PATIENT_NOT_FOUND, "Patient not found");
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

        var dto = patient.Adapt<PatientDto>();
        return Result.Success(dto);
    }
}
