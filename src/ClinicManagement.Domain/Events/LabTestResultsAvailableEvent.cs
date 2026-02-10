using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when lab test results are available
/// </summary>
public record LabTestResultsAvailableEvent(
    Guid LabTestOrderId,
    Guid PatientId,
    Guid LabTestId,
    bool IsAbnormal
) : DomainEvent;
