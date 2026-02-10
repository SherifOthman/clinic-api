namespace ClinicManagement.Application.DTOs;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string Description { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public decimal MonthlyFee { get; set; }
    public decimal YearlyFee { get; set; }
    public decimal SetupFee { get; set; }
    public int MaxBranches { get; set; }
    public int MaxStaff { get; set; }
    public bool HasAdvancedReporting { get; set; }
    public bool HasApiAccess { get; set; }
    public bool HasPrioritySupport { get; set; }
    public bool HasCustomBranding { get; set; }
    public bool IsActive { get; set; }
    public bool IsPopular { get; set; }
    public int DisplayOrder { get; set; }
}
