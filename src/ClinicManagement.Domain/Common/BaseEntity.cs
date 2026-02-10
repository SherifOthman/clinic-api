using ClinicManagement.Domain.Common.Interfaces;

using System;

namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base entity with code-generated GUID
/// ID is generated in constructor for immediate availability
/// </summary>
public abstract class BaseEntity
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; set; }
}
