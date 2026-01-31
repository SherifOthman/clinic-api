using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public int MaxUsers { get; set; }
    public int MaxPatients { get; set; }
    public int MaxClinics { get; set; } = 1;
    public int MaxBranches { get; set; } = 1;
    public bool HasAdvancedReporting { get; set; } = false;
    public bool HasApiAccess { get; set; } = false;
    public bool HasPrioritySupport { get; set; } = false;
    public bool HasCustomBranding { get; set; } = false;
    public bool IsActive { get; set; } = true;
}