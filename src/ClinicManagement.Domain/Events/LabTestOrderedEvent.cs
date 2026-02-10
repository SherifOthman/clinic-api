using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when a lab test is ordered
/// </summary>
public record LabTestOrderedEvent(
    Guid LabTestOrderId,
    Guid PatientId,
    Guid LabTestId,
    Guid? MedicalVisitId
) : DomainEvent;
