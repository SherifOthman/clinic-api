using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Stores the permissions for a specific clinic member.
/// One row per permission — the full set is the member's effective permissions.
/// Role defaults are seeded on member creation; the owner can add/remove rows.
/// Implements ISoftDeletable so the DbContext auto-applies the soft-delete filter
/// matching ClinicMember's filter (prevents EF global query filter warning).
/// </summary>
public class ClinicMemberPermission : BaseEntity, ISoftDeletable
{
    public Guid       ClinicMemberId { get; init; }
    public Permission Permission     { get; init; }
    public bool       IsDeleted      { get; set; } = false;

    // Navigation
    public ClinicMember ClinicMember { get; set; } = null!;
}
