using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class Payment : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? Note { get; set; } // For example: February installment
    public string? ReferenceNumber { get; set; } // For bank transfers, check numbers, etc.

    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
}