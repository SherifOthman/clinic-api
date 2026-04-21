using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Stores the default permissions for each ClinicMemberRole.
///
/// Design decisions:
/// - Uses ClinicMemberRole enum (not ASP.NET Identity Role) as the key.
///   ClinicMemberRole is the per-clinic role concept; Identity roles are
///   only used for JWT claims and coarse-grained policies.
/// - No FK to Identity Roles — keeps the permission system decoupled from Identity.
/// - Replaces the hardcoded DefaultPermissions.cs dictionary as the source of truth.
///   DefaultPermissions.cs is kept as a fallback during migration.
/// </summary>
public class RoleDefaultPermission
{
    public int Id { get; set; }
    public ClinicMemberRole Role { get; set; }
    public Permission Permission { get; set; }
}
