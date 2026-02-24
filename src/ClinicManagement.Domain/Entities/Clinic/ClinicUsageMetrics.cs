using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Tracks daily/monthly usage metrics against plan limits.
/// Used for enforcing subscription limits and identifying upsell opportunities.
/// </summary>
public class ClinicUsageMetrics : BaseEntity
{
    public Guid ClinicId { get; set; }
    public DateTime MetricDate { get; set; }
    public int ActiveStaffCount { get; set; } = 0;
    public int NewPatientsCount { get; set; } = 0;
    public int TotalPatientsCount { get; set; } = 0;
    public int AppointmentsCount { get; set; } = 0;
    public int InvoicesCount { get; set; } = 0;
    public decimal StorageUsedGB { get; set; } = 0;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
