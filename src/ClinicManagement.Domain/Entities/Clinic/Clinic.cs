using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Clinic : BaseEntity
{
    public string Name { get; set; } = null!;
    public Guid OwnerUserId { get; init; }
    public Guid SubscriptionPlanId { get; set; }
    public bool OnboardingCompleted { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; set; }
    
    // Subscription management (US-4)
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public string? BillingEmail { get; set; }
}
