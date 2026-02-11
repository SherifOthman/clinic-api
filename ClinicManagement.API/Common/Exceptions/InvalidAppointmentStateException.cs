using ClinicManagement.API.Common.Enums;

namespace ClinicManagement.API.Common.Exceptions;

/// <summary>
/// Exception thrown when an appointment operation is invalid for the current state
/// </summary>
public class InvalidAppointmentStateException : DomainException
{
    public AppointmentStatus CurrentStatus { get; }
    public string Operation { get; }

    public InvalidAppointmentStateException(AppointmentStatus currentStatus, string operation, string? errorCode = null) 
        : base($"Cannot {operation} appointment in {currentStatus} status", errorCode ?? string.Empty)
    {
        CurrentStatus = currentStatus;
        Operation = operation;
    }
}
