namespace ClinicManagement.Domain.Enums;

public enum AppointmentStatus
{
    Pending,
    Waiting,     // Patient arrived, waiting to be called
    InProgress,
    Completed,
    Cancelled,
    NoShow,
}
