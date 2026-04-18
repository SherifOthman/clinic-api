using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Payment : AuditableEntity, INoAuditLog
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public PaymentStatus Status { get; set; } = PaymentStatus.Paid;
    public string? Note { get; set; }
    public string? ReferenceNumber { get; set; }

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool IsPaid      => Status == PaymentStatus.Paid;
    public bool HasNote     => !string.IsNullOrWhiteSpace(Note);
    public bool HasReference => !string.IsNullOrWhiteSpace(ReferenceNumber);
}
