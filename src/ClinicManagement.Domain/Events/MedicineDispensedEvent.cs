using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when medicine is dispensed to a patient
/// </summary>
public record MedicineDispensedEvent(
    Guid DispensingId,
    Guid MedicineId,
    Guid PatientId,
    int Quantity,
    SaleUnit Unit,
    Guid? VisitId
) : DomainEvent;
