using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when radiology test results are available
/// </summary>
public record RadiologyResultsAvailableEvent(
    Guid RadiologyOrderId,
    Guid PatientId,
    Guid RadiologyTestId
) : DomainEvent;
