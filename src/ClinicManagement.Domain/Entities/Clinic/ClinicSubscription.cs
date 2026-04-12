using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class ClinicSubscription : AuditableTenantEntity
{
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public DateTimeOffset? TrialEndDate { get; set; }
    public bool AutoRenew { get; set; } = true;
    public string? CancellationReason { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? CancelledBy { get; set; }

    // Navigation properties
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
}
