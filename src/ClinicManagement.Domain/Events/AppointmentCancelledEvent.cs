using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when an appointment is cancelled
/// </summary>
public record AppointmentCancelledEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    DateTime AppointmentDate,
    string Reason
) : DomainEvent;
