using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
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
    public string? KnownAllergies { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? EmergencyContactRelation { get; init; }
    public List<PatientPhoneDto> PhoneNumbers { get; init; } = [];
    public List<PatientChronicDiseaseDto> ChronicDiseases { get; init; } = [];
    public string CreatedAt { get; init; } = null!;
}

public record PatientPhoneDto(string PhoneNumber, bool IsPrimary);
public record PatientChronicDiseaseDto(string Id, string NameEn, string NameAr);

public class GetPatientDetailHandler : IRequestHandler<GetPatientDetailQuery, Result<PatientDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPatientDetailHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<PatientDetailDto>> Handle(GetPatientDetailQuery request, CancellationToken cancellationToken)
    {
        // Nav properties exist — Include is the right choice
        var patient = await _context.Patients
            .AsNoTracking()
            .Include(p => p.Phones)
            .Include(p => p.ChronicDiseases)
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

        if (patient == null)
            return Result.Failure<PatientDetailDto>(ErrorCodes.PATIENT_NOT_FOUND, "Patient not found");

        // Chronic disease names — separate lookup, no nav property from PatientChronicDisease to ChronicDisease
        var diseaseIds = patient.ChronicDiseases.Select(cd => cd.ChronicDiseaseId).ToList();
        var diseaseNames = await _context.ChronicDiseases
            .Where(cd => diseaseIds.Contains(cd.Id))
            .Select(cd => new { cd.Id, cd.NameEn, cd.NameAr })
            .ToListAsync(cancellationToken);

        var diseaseMap = diseaseNames.ToDictionary(d => d.Id);
        var now = DateTime.UtcNow;

        return Result.Success(new PatientDetailDto
        {
            Id                     = patient.Id.ToString(),
            PatientCode            = patient.PatientCode,
            FullName               = patient.FullName,
            DateOfBirth            = patient.DateOfBirth.ToString("yyyy-MM-dd"),
            IsMale                 = patient.IsMale,
            Age                    = patient.GetAge(now),
            BloodType              = patient.BloodType?.ToString(),
            KnownAllergies         = patient.KnownAllergies,
            EmergencyContactName   = patient.EmergencyContactName,
            EmergencyContactPhone  = patient.EmergencyContactPhone,
            EmergencyContactRelation = patient.EmergencyContactRelation,
            PhoneNumbers = patient.Phones
                .Select(p => new PatientPhoneDto(p.PhoneNumber, p.IsPrimary))
                .ToList(),
            ChronicDiseases = patient.ChronicDiseases
                .Where(cd => diseaseMap.ContainsKey(cd.ChronicDiseaseId))
                .Select(cd => new PatientChronicDiseaseDto(
                    cd.ChronicDiseaseId.ToString(),
                    diseaseMap[cd.ChronicDiseaseId].NameEn,
                    diseaseMap[cd.ChronicDiseaseId].NameAr))
                .ToList(),
            CreatedAt = patient.CreatedAt.ToString("O"),
        });
    }
}
