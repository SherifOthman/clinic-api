using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Tracks subscription payment history.
/// </summary>
public class SubscriptionPayment : AuditableEntity
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
