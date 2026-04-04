using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Clinic : AuditableEntity
{
    public string Name { get; set; } = null!;
    public Guid OwnerUserId { get; init; }
    public Guid SubscriptionPlanId { get; set; }
    public bool OnboardingCompleted { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public string? BillingEmail { get; set; }

    // Navigation properties
    public User Owner { get; set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
}
