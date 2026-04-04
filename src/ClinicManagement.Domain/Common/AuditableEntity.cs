namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity with audit fields: CreatedAt, UpdatedAt, IsDeleted.
/// All entities that need soft-delete and timestamps should inherit from this.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public void SoftDelete() => IsDeleted = true;
    public void Restore() => IsDeleted = false;
    public void Touch() => UpdatedAt = DateTime.UtcNow;
}
