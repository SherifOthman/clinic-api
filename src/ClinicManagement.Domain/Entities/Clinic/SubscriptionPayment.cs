using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Tracks subscription payment history.
/// Payment records are created by gateway callbacks — no human creator.
/// Has its own PaymentDate, so CreatedAt/CreatedBy add no value.
/// Uses BaseEntity instead of AuditableEntity.
/// </summary>
public class SubscriptionPayment : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTimeOffset PaymentDate { get; set; }
    public SubscriptionPaymentStatus Status { get; set; } = SubscriptionPaymentStatus.Pending;
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset? RefundedAt { get; set; }
    public decimal? RefundAmount { get; set; }
}
