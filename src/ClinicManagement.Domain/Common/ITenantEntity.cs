namespace ClinicManagement.Domain.Common;

/// <summary>
/// Marker interface for entities that belong to a specific clinic (tenant).
/// Ensures multi-tenancy data isolation at the entity level.
/// </summary>
public interface ITenantEntity
{
    Guid ClinicId { get; set; }
}
