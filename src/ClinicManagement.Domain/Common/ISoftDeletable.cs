namespace ClinicManagement.Domain.Common;

/// <summary>
/// Marks an entity as soft-deletable with only an IsDeleted flag.
/// Use this instead of AuditableEntity when you need soft-delete behavior
/// but don't need the full audit trail (CreatedAt, UpdatedBy, etc.).
/// The DbContext automatically applies a global query filter for all types
/// implementing this interface.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    void SoftDelete() => IsDeleted = true;
    void Restore()    => IsDeleted = false;
}
