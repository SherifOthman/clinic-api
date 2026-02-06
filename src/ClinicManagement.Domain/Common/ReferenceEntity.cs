namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity for reference/lookup data with integer primary keys
/// Used for entities that reference external systems (e.g., GeoNames)
/// </summary>
public abstract class ReferenceEntity
{
    public int Id { get; set; }
}
