using ClinicManagement.API.Common.Enums;

namespace ClinicManagement.API.Common.Exceptions;

public class InvalidInvoiceStateException : DomainException
{
    public InvoiceStatus CurrentStatus { get; }
    public string Operation { get; }

    public InvalidInvoiceStateException(InvoiceStatus currentStatus, string operation) 
        : base("INVALID_STATE_TRANSITION", $"Cannot {operation} invoice in {currentStatus} status")
    {
        CurrentStatus = currentStatus;
        Operation = operation;
    }
}
