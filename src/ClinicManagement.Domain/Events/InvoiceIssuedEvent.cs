using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when an invoice is issued (status changes from Draft to Issued)
/// </summary>
public record InvoiceIssuedEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid PatientId,
    Guid ClinicBranchId,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    DateTime IssuedDate
) : DomainEvent;
