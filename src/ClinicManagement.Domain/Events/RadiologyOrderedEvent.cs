using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when a radiology test is ordered
/// </summary>
public record RadiologyOrderedEvent(
    Guid RadiologyOrderId,
    Guid PatientId,
    Guid RadiologyTestId,
    Guid? MedicalVisitId
) : DomainEvent;
