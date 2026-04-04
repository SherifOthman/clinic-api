using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries;

public record GetPatientDetailQuery(Guid PatientId) : IRequest<Result<PatientDetailDto>>;

public record PatientDetailDto
{
    public string Id { get; init; } = null!;
    public string PatientCode { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string DateOfBirth { get; init; } = null!;
    public bool IsMale { get; init; }
    public int Age { get; init; }
    public string? BloodType { get; init; }
    public int? CityGeoNameId { get; init; }
    public string? KnownAllergies { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? EmergencyContactRelation { get; init; }
    public List<PatientPhoneDto> PhoneNumbers { get; init; } = [];
    public List<PatientChronicDiseaseDto> ChronicDiseases { get; init; } = [];
    public string CreatedAt { get; init; } = null!;
    public string? UpdatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; init; }
}

public record PatientPhoneDto(string PhoneNumber, bool IsPrimary);
public record PatientChronicDiseaseDto(string Id, string NameEn, string NameAr);

public class GetPatientDetailHandler : IRequestHandler<GetPatientDetailQuery, Result<PatientDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPatientDetailHandler(IApplicationDbContext context) => _context = context;

    private static string? ToBloodTypeDisplay(BloodType? bt) => bt switch
    {
        BloodType.APositive  => "A+",
        BloodType.ANegative  => "A-",
        BloodType.BPositive  => "B+",
        BloodType.BNegative  => "B-",
        BloodType.ABPositive => "AB+",
        BloodType.ABNegative => "AB-",
        BloodType.OPositive  => "O+",
        BloodType.ONegative  => "O-",
        _                    => null,
    };

    public async Task<Result<PatientDetailDto>> Handle(GetPatientDetailQuery request, CancellationToken cancellationToken)
    {
        // Load patient with navigation properties
        var patient = await _context.Patients
            .AsNoTracking()
            .Include(p => p.Phones)
            .Include(p => p.ChronicDiseases)
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

        if (patient is null)
            return Result.Failure<PatientDetailDto>(ErrorCodes.PATIENT_NOT_FOUND, "Patient not found");

        // Resolve chronic disease names
        var phones = patient.Phones?.ToList() ?? [];
        var patientDiseases = patient.ChronicDiseases?.ToList() ?? [];
        var diseaseIds = patientDiseases.Select(cd => cd.ChronicDiseaseId).ToList();

        var diseaseMap = diseaseIds.Count > 0
            ? await _context.ChronicDiseases
                .Where(cd => diseaseIds.Contains(cd.Id))
                .Select(cd => new { cd.Id, cd.NameEn, cd.NameAr })
                .ToDictionaryAsync(d => d.Id, cancellationToken)
            : [];

        // Resolve audit user names
        var userIds = new[] { patient.CreatedBy, patient.UpdatedBy }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var userNames = userIds.Count > 0
            ? await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
                .ToDictionaryAsync(u => u.Id, u => u.Name.Trim(), cancellationToken)
            : [];

        var now = DateTime.UtcNow;

        return Result.Success(new PatientDetailDto
        {
            Id                       = patient.Id.ToString(),
            PatientCode              = patient.PatientCode,
            FullName                 = patient.FullName,
            DateOfBirth              = patient.DateOfBirth.ToString("yyyy-MM-dd"),
            IsMale                   = patient.IsMale,
            Age                      = patient.GetAge(now),
            BloodType                = ToBloodTypeDisplay(patient.BloodType),
            CityGeoNameId            = patient.CityGeoNameId,
            KnownAllergies           = patient.KnownAllergies,
            EmergencyContactName     = patient.EmergencyContactName,
            EmergencyContactPhone    = patient.EmergencyContactPhone,
            EmergencyContactRelation = patient.EmergencyContactRelation,
            PhoneNumbers             = phones
                                        .Select(p => new PatientPhoneDto(p.PhoneNumber, p.IsPrimary))
                                        .ToList(),
            ChronicDiseases          = patientDiseases
                                        .Where(cd => diseaseMap.ContainsKey(cd.ChronicDiseaseId))
                                        .Select(cd => new PatientChronicDiseaseDto(
                                            cd.ChronicDiseaseId.ToString(),
                                            diseaseMap[cd.ChronicDiseaseId].NameEn,
                                            diseaseMap[cd.ChronicDiseaseId].NameAr))
                                        .ToList(),
            CreatedAt = patient.CreatedAt.ToString("O"),
            UpdatedAt = patient.UpdatedAt?.ToString("O"),
            CreatedBy = patient.CreatedBy.HasValue && userNames.TryGetValue(patient.CreatedBy.Value, out var cn) ? cn : null,
            UpdatedBy = patient.UpdatedBy.HasValue && userNames.TryGetValue(patient.UpdatedBy.Value, out var un) ? un : null,
        });
    }
}
