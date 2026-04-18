using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Invoice : TenantEntity
{
    public string InvoiceNumber { get; set; } = null!;
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? MedicalVisitId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTimeOffset? IssuedDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string? Notes { get; set; }

    // ── Computed ──────────────────────────────────────────────────────────────

    public decimal NetAmount => TotalAmount - Discount + TaxAmount;

    public bool IsDraft        => Status == InvoiceStatus.Draft;
    public bool IsIssued       => Status == InvoiceStatus.Issued;
    public bool IsPartiallyPaid => Status == InvoiceStatus.PartiallyPaid;
    public bool IsPaid         => Status == InvoiceStatus.FullyPaid;
    public bool IsCancelled    => Status == InvoiceStatus.Cancelled;
    public bool IsOverdue(DateTimeOffset now) =>
        DueDate.HasValue && now > DueDate.Value &&
        Status != InvoiceStatus.FullyPaid && Status != InvoiceStatus.Cancelled;

    public bool CanBeIssued    => Status == InvoiceStatus.Draft;
    public bool CanBeCancelled => Status != InvoiceStatus.FullyPaid && Status != InvoiceStatus.Cancelled;

    /// <summary>Days until due date. Negative means overdue. Null if no due date.</summary>
    public int? DaysUntilDue(DateTimeOffset now) =>
        DueDate.HasValue ? (int)(DueDate.Value - now).TotalDays : null;
}
