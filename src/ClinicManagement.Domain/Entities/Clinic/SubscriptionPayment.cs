using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Tracks subscription payment history.
/// Records payment attempts, status, and transaction details.
/// </summary>
public class SubscriptionPayment : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PaymentDate { get; set; }
    public SubscriptionPaymentStatus Status { get; set; } = SubscriptionPaymentStatus.Pending;
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; set; }
    
    public void SetCreatedAt(DateTime createdAt) => CreatedAt = createdAt;
}
