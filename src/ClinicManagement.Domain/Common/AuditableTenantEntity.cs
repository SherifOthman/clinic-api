namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity for tenant-scoped entities with audit tracking.
/// Includes ClinicId for multi-tenancy and CreatedAt/IsDeleted for auditing.
/// </summary>
public abstract class AuditableTenantEntity : AuditableEntity, ITenantEntity
{
    public Guid ClinicId { get; set; }
}
