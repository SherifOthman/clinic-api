namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity with int primary key (database identity).
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}
