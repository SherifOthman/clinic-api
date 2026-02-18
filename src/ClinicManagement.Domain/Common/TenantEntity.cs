namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity for tenant-scoped entities with audit fields.
/// Automatically includes ClinicId for multi-tenancy isolation.
/// </summary>
public abstract class TenantEntity : AuditableEntity, ITenantEntity
{
    public int ClinicId { get; set; }
}
