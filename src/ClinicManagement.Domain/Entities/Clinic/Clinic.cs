using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Clinic : AuditableEntity, ISoftDeletable, IAuditableEntity
{
    public string Name { get; set; } = null!;
    public Guid OwnerUserId { get; init; }
    public Guid SubscriptionPlanId { get; set; }
    public bool OnboardingCompleted { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? SubscriptionStartDate { get; set; }
    public DateTimeOffset? SubscriptionEndDate { get; set; }
    public DateTimeOffset? TrialEndDate { get; set; }
    public string? BillingEmail { get; set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g. "EG", "SA", "US").
    /// Set during onboarding — used as the default region for phone number
    /// normalization so local-format search works correctly.
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// First day of the week for calendar display (0 = Sunday … 6 = Saturday).
    /// Defaults to 6 (Saturday) — common in Middle-East clinics.
    /// </summary>
    public int WeekStartDay { get; set; } = 6;

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool IsOnboarded => OnboardingCompleted;

    public bool IsTrialActive(DateTimeOffset now) =>
        TrialEndDate.HasValue && now < TrialEndDate.Value;

    public bool IsSubscriptionActive(DateTimeOffset now) =>
        IsActive && (!SubscriptionEndDate.HasValue || now < SubscriptionEndDate.Value);

    /// <summary>Days until subscription expires. Null if no end date. Negative means expired.</summary>
    public int? DaysUntilSubscriptionEnd(DateTimeOffset now) =>
        SubscriptionEndDate.HasValue ? (int)(SubscriptionEndDate.Value - now).TotalDays : null;

    /// <summary>Days until trial ends. Null if no trial. Negative means trial expired.</summary>
    public int? DaysUntilTrialEnd(DateTimeOffset now) =>
        TrialEndDate.HasValue ? (int)(TrialEndDate.Value - now).TotalDays : null;

    // Navigation properties
    public User Owner { get; set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public ICollection<ClinicBranch> Branches { get; set; } = new List<ClinicBranch>();
    public ICollection<ClinicMember> Members { get; set; } = new List<ClinicMember>();
}
