using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A user's membership at a specific clinic with a specific role.
/// UserId is nullable — set when the person accepts their invitation and creates an account.
/// Role is per-clinic — same user can be Doctor at Clinic A, Receptionist at Clinic B.
/// </summary>
public class ClinicMember : AuditableTenantEntity, IAuditableEntity, ISoftDeletable
{
    /// <summary>Nullable — set when the invitation is accepted and the user account is created.</summary>
    public Guid? UserId { get; set; }

    public ClinicMemberRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool IsOwner        => Role == ClinicMemberRole.Owner;
    public bool IsDoctor       => Role == ClinicMemberRole.Doctor;
    public bool IsReceptionist => Role == ClinicMemberRole.Receptionist;
    public bool IsNurse        => Role == ClinicMemberRole.Nurse;

    // Navigation
    public User? User { get; set; }
    public Clinic Clinic { get; set; } = null!;
    public DoctorInfo? DoctorInfo { get; set; }
    public ICollection<ClinicMemberPermission> Permissions { get; set; } = new List<ClinicMemberPermission>();
}
