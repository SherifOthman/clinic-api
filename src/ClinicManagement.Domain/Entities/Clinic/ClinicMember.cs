using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A person's membership at a specific clinic with a specific role.
/// Replaces the old Staff entity.
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

    // Navigation
    public Person Person { get; set; } = null!;
    public User? User { get; set; }
    public DoctorInfo? DoctorInfo { get; set; }
}
