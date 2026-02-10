using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when a doctor reviews radiology test results
/// </summary>
public record RadiologyReviewedEvent(
    Guid RadiologyOrderId,
    Guid PatientId,
    Guid ReviewedByDoctorId
) : DomainEvent;
