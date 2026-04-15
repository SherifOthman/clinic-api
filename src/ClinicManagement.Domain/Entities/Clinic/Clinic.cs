using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Clinic : AuditableEntity
{
    public string Name { get; set; } = null!;
    public Guid OwnerUserId { get; init; }
    public Guid SubscriptionPlanId { get; set; }
    public bool OnboardingCompleted { get; set; }
    public bool IsActive { get; set; } = true;
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

    // Navigation properties
    public User Owner { get; set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    public ICollection<ClinicBranch> Branches { get; set; } = new List<ClinicBranch>();

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();}
