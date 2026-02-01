using ClinicManagement.Domain.Common.Interfaces;

namespace ClinicManagement.Domain.Common;

public abstract class BaseEntity : IEntity
{
    public Guid Id { get; set; }
}
