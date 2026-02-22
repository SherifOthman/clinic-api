namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity for tenant-scoped entities.
/// Automatically includes ClinicId for multi-tenancy isolation.
/// </summary>
public abstract class TenantEntity : BaseEntity, ITenantEntity
{
    public int ClinicId { get; set; }
}
