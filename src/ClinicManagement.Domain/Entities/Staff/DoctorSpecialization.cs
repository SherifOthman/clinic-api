using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Supports multiple specializations per doctor.
/// Tracks certification details and years of experience per specialization.
/// </summary>
public class DoctorSpecialization : BaseEntity
{
    public Guid DoctorProfileId { get; set; }
    public Guid SpecializationId { get; set; }
    public bool IsPrimary { get; set; } = false;
    public int YearsOfExperience { get; set; } = 0;
    public string? CertificationNumber { get; set; }
    public DateTime? CertificationDate { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
}
