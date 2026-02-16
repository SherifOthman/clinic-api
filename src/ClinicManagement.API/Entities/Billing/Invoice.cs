using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;

namespace ClinicManagement.API.Entities;

public class Invoice : TenantEntity
{
    private readonly List<InvoiceItem> _items = [];
    private readonly List<Payment> _payments = [];

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

    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Appointment? Appointment { get; set; }
    public MedicalVisit? MedicalVisit { get; set; }
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    // Calculated properties
    public decimal SubtotalAmount => _items.Sum(i => i.LineTotal);
    public decimal FinalAmount => SubtotalAmount - Discount + TaxAmount;
    public decimal TotalPaid => _payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
    public decimal RemainingAmount => FinalAmount - TotalPaid;
    public bool IsFullyPaid => RemainingAmount <= 0;
    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && !IsFullyPaid && Status != InvoiceStatus.Cancelled;
    public bool IsPartiallyPaid => TotalPaid > 0 && !IsFullyPaid;
    public decimal DiscountPercentage => SubtotalAmount > 0 ? Math.Round((Discount / SubtotalAmount) * 100, 2) : 0;
    public int DaysOverdue => IsOverdue && DueDate.HasValue ? (DateTime.UtcNow.Date - DueDate.Value.Date).Days : 0;
}
