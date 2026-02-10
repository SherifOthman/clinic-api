using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when an invoice becomes fully paid
/// </summary>
public record InvoiceFullyPaidEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid PatientId,
    decimal TotalAmount,
    decimal TotalPaid,
    DateTime FullyPaidDate
) : DomainEvent;
