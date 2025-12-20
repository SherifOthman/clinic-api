using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Clinic : AuditableEntity
{
    public int OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool OnboardingCompleted { get; set; } = false;
    public virtual User Owner { get; set; } = null!;
}
