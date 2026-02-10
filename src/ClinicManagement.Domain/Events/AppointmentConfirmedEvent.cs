using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when an appointment is confirmed
/// </summary>
public record AppointmentConfirmedEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    DateTime AppointmentDate,
    short QueueNumber
) : DomainEvent;
