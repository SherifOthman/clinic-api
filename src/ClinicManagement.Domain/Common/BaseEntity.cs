namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity with GUID primary key.
/// ID is generated in constructor for immediate availability.
/// </summary>
public abstract class BaseEntity
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; set; }
}
