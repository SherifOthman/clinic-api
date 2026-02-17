namespace ClinicManagement.Domain.Common;

/// <summary>
/// Marker interface for entities that belong to a specific clinic (tenant).
/// Ensures multi-tenancy data isolation at the entity level.
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// The clinic (tenant) this entity belongs to.
    /// Automatically set by ApplicationDbContext on entity creation.
    /// </summary>
    Guid ClinicId { get; set; }
}
