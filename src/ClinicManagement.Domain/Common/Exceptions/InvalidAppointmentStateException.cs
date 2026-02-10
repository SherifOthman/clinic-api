using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when an appointment operation is invalid for the current state
/// </summary>
public class InvalidAppointmentStateException : DomainException
{
    public AppointmentStatus CurrentStatus { get; }
    public string Operation { get; }

    public InvalidAppointmentStateException(AppointmentStatus currentStatus, string operation, string errorCode) 
        : base($"Cannot {operation} appointment in {currentStatus} status", errorCode)
    {
        CurrentStatus = currentStatus;
        Operation = operation;
    }
}
