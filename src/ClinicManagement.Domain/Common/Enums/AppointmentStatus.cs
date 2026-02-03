namespace ClinicManagement.Domain.Common.Enums;

public enum AppointmentStatus: byte
{
    Pending = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}
