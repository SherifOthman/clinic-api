namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity with full audit trail: who created/updated and when.
/// CreatedAt, CreatedBy, UpdatedAt, UpdatedBy are stamped automatically
/// by ApplicationDbContext.StampAuditFields on every SaveChangesAsync call.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>UserId of the user who created this record.</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>UserId of the user who last updated this record.</summary>
    public Guid? UpdatedBy { get; set; }
}
