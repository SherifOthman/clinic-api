using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when medicine dispensing is cancelled
/// </summary>
public record MedicineDispensingCancelledEvent(
    Guid DispensingId,
    Guid MedicineId,
    Guid PatientId,
    int Quantity,
    SaleUnit Unit,
    string Reason
) : DomainEvent;
