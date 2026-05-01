using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents a real human being in the system.
/// A Person can be a ClinicMember (staff), a Patient, or both.
/// Personal data lives here once — no duplication across roles.
/// </summary>
public class Person : BaseEntity
{

    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? ProfileImageUrl { get; set; }

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool HasProfileImage => !string.IsNullOrWhiteSpace(ProfileImageUrl);

    // Navigation
    public User? User { get; set; }
    public ICollection<ClinicMember> ClinicMemberships { get; set; } = new List<ClinicMember>();
    public ICollection<Patient> PatientRecords { get; set; } = new List<Patient>();
}