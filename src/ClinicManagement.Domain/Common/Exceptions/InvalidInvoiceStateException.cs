using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when an invoice operation is invalid for the current state
/// </summary>
public class InvalidInvoiceStateException : DomainException
{
    public InvoiceStatus CurrentStatus { get; }
    public string Operation { get; }

    public InvalidInvoiceStateException(InvoiceStatus currentStatus, string operation, string errorCode) 
        : base($"Cannot {operation} invoice in {currentStatus} status", errorCode)
    {
        CurrentStatus = currentStatus;
        Operation = operation;
    }
}
