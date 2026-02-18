using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Invoice : TenantEntity
{
    public int PatientId { get; set; }
    public int? AppointmentId { get; set; }
    public int? MedicalVisitId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime? IssuedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
}
