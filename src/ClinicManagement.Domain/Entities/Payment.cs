using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Payment records for VisitServiceItems.
/// Supports partial payments and installments.
/// </summary>
public class Payment : AuditableEntity
{
    public Guid VisitServiceItemId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; } // Cash, Card, Transfer, Check, Insurance
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; } // Transaction ID, check number, etc.
    
    // Navigation properties
    public VisitServiceItem VisitServiceItem { get; set; } = null!;
}