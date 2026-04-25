using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Stores the permissions for a specific clinic member.
/// One row per permission — the full set is the member's effective permissions.
/// Role defaults are seeded on member creation; the owner can add/remove rows.
/// </summary>
public class ClinicMemberPermission : BaseEntity
{
    public Guid ClinicMemberId { get; init; }
    public Permission Permission { get; init; }

    // Navigation
    public ClinicMember ClinicMember { get; set; } = null!;
}
