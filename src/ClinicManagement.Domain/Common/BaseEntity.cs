namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity with Guid primary key generated in code.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
}
