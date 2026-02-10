using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when medicine stock falls below minimum level
/// </summary>
public record MedicineStockLowEvent(
    Guid MedicineId,
    string MedicineName,
    int CurrentStock,
    int MinimumStockLevel,
    int ReorderLevel,
    bool NeedsReorder
) : DomainEvent;
