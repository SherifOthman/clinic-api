using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class Invoice : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid ClinicPatientId { get; set; }
    
    // Optional link to visit or appointment
    public Guid? MedicalVisitId { get; set; }
    public MedicalVisit? MedicalVisit { get; set; }
    
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalAmount => TotalAmount - Discount;
    
    // Invoice status and dates
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime? IssuedDate { get; set; }
    public DateTime? DueDate { get; set; }

    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public ClinicPatient ClinicPatient { get; set; } = null!;
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    // Installment logic
    public decimal TotalPaid => Payments.Sum(p => p.Amount);
    public decimal RemainingAmount => FinalAmount - TotalPaid;
    public bool IsFullyPaid => RemainingAmount <= 0;
    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && !IsFullyPaid;
}