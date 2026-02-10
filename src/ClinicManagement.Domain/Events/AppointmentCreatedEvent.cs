using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when a new appointment is created
/// </summary>
public record AppointmentCreatedEvent(
    Guid AppointmentId,
    Guid ClinicBranchId,
    Guid PatientId,
    Guid DoctorId,
    string AppointmentNumber,
    DateTime AppointmentDate,
    short QueueNumber
) : DomainEvent;
