using ClinicManagement.Domain.Common.Interfaces;

namespace ClinicManagement.Domain.Common;

/// <summary>
/// Base class for aggregate roots
/// Aggregate roots are the entry points to aggregates and are responsible for maintaining invariants
/// They collect domain events that are dispatched after the transaction commits
/// </summary>
public abstract class AggregateRoot : AuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the domain events that have been raised by this aggregate
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to be dispatched after the transaction commits
    /// </summary>
    /// <param name="domainEvent">The domain event to add</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a specific domain event
    /// </summary>
    /// <param name="domainEvent">The domain event to remove</param>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events
    /// Called by the infrastructure after events are dispatched
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
