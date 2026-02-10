using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when a payment operation is invalid for the current state
/// </summary>
public class InvalidPaymentStateException : DomainException
{
    public PaymentStatus CurrentStatus { get; }
    public string Operation { get; }

    public InvalidPaymentStateException(PaymentStatus currentStatus, string operation, string errorCode) 
        : base($"Cannot {operation} payment in {currentStatus} status", errorCode)
    {
        CurrentStatus = currentStatus;
        Operation = operation;
    }
}
