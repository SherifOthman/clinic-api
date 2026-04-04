namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity with full audit trail: who created/updated and when.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    /// <summary>UserId of the user who created this record.</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>UserId of the user who last updated this record.</summary>
    public Guid? UpdatedBy { get; set; }

    public void SoftDelete() => IsDeleted = true;
    public void Restore() => IsDeleted = false;
    public void Touch() => UpdatedAt = DateTime.UtcNow;
}
