using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when an appointment is completed
/// </summary>
public record AppointmentCompletedEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    DateTime AppointmentDate
) : DomainEvent;
