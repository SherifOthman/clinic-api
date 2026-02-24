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
    public DateTime? IssuedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }

    public decimal NetAmount => TotalAmount - Discount + TaxAmount;
    public bool IsDraft => Status == InvoiceStatus.Draft;
    public bool IsPaid => Status == InvoiceStatus.FullyPaid;
    public bool IsCancelled => Status == InvoiceStatus.Cancelled;
    public bool IsOverdue(DateTime currentDate) => DueDate.HasValue && currentDate > DueDate.Value && Status != InvoiceStatus.FullyPaid && Status != InvoiceStatus.Cancelled;
}
