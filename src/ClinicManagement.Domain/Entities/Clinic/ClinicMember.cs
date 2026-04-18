using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A person's membership at a specific clinic with a specific role.
///
/// Key design decisions:
/// - PersonId required — every member is a real person
/// - UserId nullable — set when the person accepts their invitation
/// - Role is per-clinic — same person can be Doctor at Clinic A, Receptionist at Clinic B
/// - One person can have multiple memberships across different clinics
/// </summary>
public class ClinicMember : AuditableTenantEntity
{
    public Guid PersonId { get; init; }

    /// <summary>Nullable — set when the person accepts their invitation and creates an account.</summary>
    public Guid? UserId { get; set; }

    public ClinicMemberRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool IsOwner        => Role == ClinicMemberRole.Owner;
    public bool IsDoctor       => Role == ClinicMemberRole.Doctor;
    public bool IsReceptionist => Role == ClinicMemberRole.Receptionist;
    public bool IsNurse        => Role == ClinicMemberRole.Nurse;

    /// <summary>True once the person has registered an account and linked it to this membership.</summary>
    public bool HasAccount => UserId.HasValue;

    public int DaysActive => (int)(DateTimeOffset.UtcNow - JoinedAt).TotalDays;

    // Navigation
    public Person Person { get; set; } = null!;
    public User? User { get; set; }
    public DoctorInfo? DoctorInfo { get; set; }
}
