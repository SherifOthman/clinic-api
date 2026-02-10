using ClinicManagement.Domain.Common.Interfaces;

namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base class for domain events
/// Provides common properties like OccurredOn timestamp
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
