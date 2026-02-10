using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when stock is added to a medicine
/// </summary>
public record MedicineStockAddedEvent(
    Guid MedicineId,
    string MedicineName,
    int StripsAdded,
    int NewTotalStock,
    string? Reason
) : DomainEvent;
