using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Tracks daily usage metrics against plan limits.
/// Written by a background job — no user context, so CreatedBy/UpdatedBy are always null.
/// Uses BaseEntity + ITenantEntity directly instead of AuditableTenantEntity.
/// LastAggregatedAt tracks when the job last ran for this record.
/// </summary>
public class ClinicUsageMetrics : BaseEntity, ITenantEntity, ISoftDeletable
{
    public Guid ClinicId { get; set; }
    public DateOnly MetricDate { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int ActiveStaffCount { get; set; }
    public int NewPatientsCount { get; set; }
    public int TotalPatientsCount { get; set; }
    public int AppointmentsCount { get; set; }
    public int InvoicesCount { get; set; }
    public decimal StorageUsedGB { get; set; }
    /// <summary>UTC timestamp of the last aggregation run for this record.</summary>
    public DateTimeOffset LastAggregatedAt { get; set; } = DateTimeOffset.UtcNow;
}
