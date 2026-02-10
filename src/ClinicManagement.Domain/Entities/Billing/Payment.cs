using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class Payment : AuditableEntity
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public PaymentStatus Status { get; set; } = PaymentStatus.Paid;
    public string? Note { get; set; }
    public string? ReferenceNumber { get; set; }

    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
}
