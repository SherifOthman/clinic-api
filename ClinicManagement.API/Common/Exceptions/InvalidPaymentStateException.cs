using ClinicManagement.API.Common.Enums;

namespace ClinicManagement.API.Common.Exceptions;

public class InvalidPaymentStateException : DomainException
{
    public PaymentStatus CurrentStatus { get; }
    public string Operation { get; }

    public InvalidPaymentStateException(PaymentStatus currentStatus, string operation) 
        : base("INVALID_STATE_TRANSITION", $"Cannot {operation} payment in {currentStatus} status")
    {
        CurrentStatus = currentStatus;
        Operation = operation;
    }
}
