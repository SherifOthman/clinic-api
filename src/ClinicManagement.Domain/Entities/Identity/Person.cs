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
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? ProfileImageUrl { get; set; }

    // ── Computed ──────────────────────────────────────────────────────────────

    public string FullName => $"{FirstName} {LastName}".Trim();

    public bool HasProfileImage => !string.IsNullOrWhiteSpace(ProfileImageUrl);

    /// <summary>Age in full years, or null if DateOfBirth is not set.</summary>
    public int? Age
    {
        get
        {
            if (DateOfBirth is not { } dob) return null;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age   = today.Year - dob.Year;
            if (dob.AddYears(age) > today) age--;
            return age;
        }
    }

    // Navigation
    public User? User { get; set; }
    public ICollection<ClinicMember> ClinicMemberships { get; set; } = new List<ClinicMember>();
    public ICollection<Patient> PatientRecords { get; set; } = new List<Patient>();
}
