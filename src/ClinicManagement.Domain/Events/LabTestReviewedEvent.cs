using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when a doctor reviews lab test results
/// </summary>
public record LabTestReviewedEvent(
    Guid LabTestOrderId,
    Guid PatientId,
    Guid ReviewedByDoctorId
) : DomainEvent;
