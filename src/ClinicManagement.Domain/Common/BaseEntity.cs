using ClinicManagement.Domain.Common.Interfaces;

namespace ClinicManagement.Domain.Common;

public abstract class BaseEntity : IEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

}
