using ClinicManagement.API.Common.Enums;

namespace ClinicManagement.API.Common.Exceptions;

public class InvalidAppointmentStateException : DomainException
{
    public AppointmentStatus CurrentStatus { get; }
    public string Operation { get; }

    public InvalidAppointmentStateException(AppointmentStatus currentStatus, string operation) 
        : base("INVALID_STATE_TRANSITION", $"Cannot {operation} appointment in {currentStatus} status")
    {
        CurrentStatus = currentStatus;
        Operation = operation;
    }
}
