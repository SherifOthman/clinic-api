using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when a new medicine is created
/// </summary>
public record MedicineCreatedEvent(
    Guid MedicineId,
    Guid ClinicBranchId,
    string Name,
    decimal BoxPrice,
    int StripsPerBox,
    int InitialStock
) : DomainEvent;
