namespace ClinicManagement.Domain.Common;

/// <summary>
/// Tenant-scoped entity with full audit trail.
/// Use for entities that belong to a clinic and need CreatedAt/UpdatedAt.
/// Soft-delete behavior is opt-in via ISoftDeletable.
/// </summary>
public abstract class AuditableTenantEntity : AuditableEntity, ITenantEntity
{
    public Guid ClinicId { get; set; }
}
