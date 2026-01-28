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
    public bool IsActive { get; set; } = true;
}