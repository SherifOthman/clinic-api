using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Tracks daily/monthly usage metrics against plan limits.
/// </summary>
public class ClinicUsageMetrics : AuditableTenantEntity, INoAuditLog, ISoftDeletable
{
    public DateOnly MetricDate { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int ActiveStaffCount { get; set; }
    public int NewPatientsCount { get; set; }
    public int TotalPatientsCount { get; set; }
    public int AppointmentsCount { get; set; }
    public int InvoicesCount { get; set; }
    public decimal StorageUsedGB { get; set; }
}
