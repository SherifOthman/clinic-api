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

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool IsActive  => Status == SubscriptionStatus.Active;
    public bool IsTrial   => Status == SubscriptionStatus.Trial;
    public bool IsCancelled => Status == SubscriptionStatus.Cancelled;
    public bool IsExpired => Status == SubscriptionStatus.Expired;

    public bool IsTrialActive(DateTimeOffset now) =>
        IsTrial && TrialEndDate.HasValue && now < TrialEndDate.Value;

    public bool IsSubscriptionActive(DateTimeOffset now) =>
        IsActive && (!EndDate.HasValue || now < EndDate.Value);

    public bool CanBeCancelled => Status != SubscriptionStatus.Cancelled && Status != SubscriptionStatus.Expired;

    /// <summary>Days remaining until end date. Null if no end date. Negative means expired.</summary>
    public int? DaysRemaining(DateTimeOffset now) =>
        EndDate.HasValue ? (int)(EndDate.Value - now).TotalDays : null;

    // Navigation properties
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;

}
