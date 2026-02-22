using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Clinic : BaseEntity
{
    public string Name { get; set; } = null!;
    public int OwnerUserId { get; set; }
    public int SubscriptionPlanId { get; set; }
    public bool OnboardingCompleted { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
