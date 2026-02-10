using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when stock is removed from a medicine
/// </summary>
public record MedicineStockRemovedEvent(
    Guid MedicineId,
    string MedicineName,
    int StripsRemoved,
    int NewTotalStock,
    string? Reason
) : DomainEvent;
