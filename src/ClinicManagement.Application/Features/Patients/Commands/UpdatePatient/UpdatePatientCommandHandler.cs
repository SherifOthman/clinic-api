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

        // Enrich the audit log entry with chronic diseases
        // (PatientChronicDisease is BaseEntity, not AuditableEntity, so not auto-tracked)
        if (request.ChronicDiseaseIds != null)
        {
            var auditEntry = await _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefaultAsync(a => a.EntityId == patient.Id.ToString() && a.Action == AuditAction.Update, cancellationToken);

            if (auditEntry != null && auditEntry.Changes != null)
            {
                var changes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(auditEntry.Changes) ?? new();

                if (request.ChronicDiseaseIds.Any())
                {
                    var diseaseNames = await _context.ChronicDiseases
                        .Where(d => request.ChronicDiseaseIds.Contains(d.Id))
                        .Select(d => d.NameEn)
                        .ToListAsync(cancellationToken);
                    changes["Chronic Diseases"] = string.Join(", ", diseaseNames);
                }
                else
                {
                    changes["Chronic Diseases"] = "None";
                }

                auditEntry.Changes = System.Text.Json.JsonSerializer.Serialize(changes);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        var dto = patient.Adapt<PatientDto>();
        return Result.Success(dto);
    }
}
