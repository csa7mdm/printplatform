namespace PrintPlatform.Domain.Shared;

/// <summary>
/// Base class for aggregate roots.
/// Inherits audit fields from <see cref="BaseEntity{TId}"/> and manages
/// a private list of pending domain events.
/// </summary>
public abstract class BaseAggregateRoot<TId> : BaseEntity<TId>, IAggregateRoot
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Registers a new domain event to be dispatched after the unit of work commits.</summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> PopDomainEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }
}
