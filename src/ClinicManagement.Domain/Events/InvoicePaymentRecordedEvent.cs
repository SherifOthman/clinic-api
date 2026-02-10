using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when a payment is recorded for an invoice
/// </summary>
public record InvoicePaymentRecordedEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid PatientId,
    decimal PaymentAmount,
    PaymentMethod PaymentMethod,
    decimal RemainingAmount,
    bool IsFullyPaid
) : DomainEvent;
