using ClinicManagement.Domain.Common.Interfaces;

using System;

namespace ClinicManagement.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
}
