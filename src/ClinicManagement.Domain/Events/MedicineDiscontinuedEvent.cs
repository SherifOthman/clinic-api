using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when a medicine is discontinued
/// </summary>
public record MedicineDiscontinuedEvent(
    Guid MedicineId,
    string MedicineName,
    int RemainingStock,
    string? Reason
) : DomainEvent;
