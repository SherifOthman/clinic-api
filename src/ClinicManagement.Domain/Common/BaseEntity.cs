namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity with a time-ordered (v7) Guid primary key.
/// Guid.CreateVersion7() generates sequential GUIDs that avoid index fragmentation
/// on SQL Server clustered indexes — same benefit as NEWSEQUENTIALID() but in code.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
}
