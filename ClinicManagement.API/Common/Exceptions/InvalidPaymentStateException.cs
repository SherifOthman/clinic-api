using ClinicManagement.API.Common.Enums;

namespace ClinicManagement.API.Common.Exceptions;

/// <summary>
/// Exception thrown when a payment operation is invalid for the current state
/// </summary>
public class InvalidPaymentStateException : DomainException
{
    public PaymentStatus CurrentStatus { get; }
    public string Operation { get; }

    public InvalidPaymentStateException(PaymentStatus currentStatus, string operation, string? errorCode = null) 
        : base($"Cannot {operation} payment in {currentStatus} status", errorCode ?? string.Empty)
    {
        CurrentStatus = currentStatus;
        Operation = operation;
    }
}
