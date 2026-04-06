using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static ClinicManagement.Domain.Enums.BloodTypeExtensions;

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
            .Include(p => p.ChronicDiseases)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (patient is null)
        {
            return Result.Failure<PatientDto>(ErrorCodes.PATIENT_NOT_FOUND, "Patient not found");
        }

        var bloodType = ParseBloodType(request.BloodType);

        patient.FullName = request.FullName;
        patient.DateOfBirth = DateTime.Parse(request.DateOfBirth);
        patient.IsMale = request.Gender == "Male";
        patient.CountryGeoNameId = request.CountryGeoNameId;
        patient.StateGeoNameId = request.StateGeoNameId;
        patient.CityGeoNameId = request.CityGeoNameId;
        patient.BloodType = bloodType;

        // Update chronic diseases if provided
        if (request.ChronicDiseaseIds != null)
        {
            // Remove all existing, add new ones
            var existing = patient.ChronicDiseases?.ToList() ?? [];
            foreach (var cd in existing)
                _context.PatientChronicDiseases.Remove(cd);

            foreach (var diseaseId in request.ChronicDiseaseIds)
                _context.PatientChronicDiseases.Add(new PatientChronicDisease
                {
                    PatientId = patient.Id,
                    ChronicDiseaseId = diseaseId,
                });
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = patient.Adapt<PatientDto>();
        return Result.Success(dto);
    }
}
