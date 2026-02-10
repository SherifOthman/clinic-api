using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when an invoice is cancelled
/// </summary>
public record InvoiceCancelledEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid PatientId,
    decimal TotalAmount,
    string Reason
) : DomainEvent;
