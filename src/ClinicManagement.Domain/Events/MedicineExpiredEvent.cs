using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when a medicine expires
/// </summary>
public record MedicineExpiredEvent(
    Guid MedicineId,
    string MedicineName,
    DateTime ExpiryDate,
    int RemainingStock
) : DomainEvent;
