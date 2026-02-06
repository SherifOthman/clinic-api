using ClinicManagement.Domain.Common.Interfaces;

using System;

namespace ClinicManagement.Domain.Common;

public abstract class BaseEntity
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; set; }
}
