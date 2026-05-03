using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Inherits AuditableTenantEntity so the global tenant query filter applies automatically.
/// Previously used AuditableEntity (no ClinicId), which meant tenant isolation
/// depended entirely on BranchId — a single-hop FK, not a direct filter.
/// </summary>
public class Appointment : AuditableTenantEntity, IAuditableEntity, ISoftDeletable
{
    public Guid BranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorInfoId { get; set; }
    public Guid VisitTypeId { get; set; }

    public DateOnly Date { get; set; }
    public int? QueueNumber { get; set; }
    public TimeOnly? ScheduledTime { get; set; }
    /// <summary>Calculated end time = ScheduledTime + visit duration. Null for queue appointments.</summary>
    public TimeOnly? EndTime { get; set; }
    /// <summary>Per-appointment duration override in minutes. Null = use doctor's default.</summary>
    public int? VisitDurationMinutes { get; set; }

    public AppointmentType Type { get; set; } = AppointmentType.Queue;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public decimal Price { get; set; }
    public decimal? DiscountPercent { get; set; }

    /// <summary>
    /// Stored final price — always set via ApplyPrice() to stay in sync with Price and DiscountPercent.
    /// Never set directly; use ApplyPrice() when creating or updating pricing.
    /// </summary>
    public decimal FinalPrice { get; private set; }

    public Guid? InvoiceId { get; set; }
    public bool IsDeleted { get; set; } = false;

    public void ApplyPrice(decimal price, decimal? discountPercent = null)
    {
        Price           = price;
        DiscountPercent = discountPercent;
        FinalPrice      = discountPercent.HasValue
            ? Math.Round(price * (1 - discountPercent.Value / 100m), 2)
            : price;
    }

    // Navigation
    public ClinicBranch Branch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public DoctorInfo Doctor { get; set; } = null!;
    public VisitType VisitType { get; set; } = null!;
}
