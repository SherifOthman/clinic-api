using MediatR;

namespace ClinicManagement.Domain.Common.Interfaces;

/// <summary>
/// Marker interface for domain events
/// Domain events represent something that happened in the domain that domain experts care about
/// They are dispatched after the transaction commits to ensure consistency
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// When the event occurred
    /// </summary>
    DateTime OccurredOn { get; }
}
