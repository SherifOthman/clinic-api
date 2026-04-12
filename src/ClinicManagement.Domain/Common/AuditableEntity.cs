namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity with full audit trail: who created/updated and when.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    /// <summary>UserId of the user who created this record.</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>UserId of the user who last updated this record.</summary>
    public Guid? UpdatedBy { get; set; }

    public void SoftDelete()
    {
        IsDeleted = true;
        Touch();
    }

    public void Restore()
    {
        IsDeleted = false;
        Touch();
    }

    public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
