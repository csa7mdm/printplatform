namespace PrintPlatform.Domain.Shared;

/// <summary>
/// Marks an entity as the root of a DDD aggregate.
/// Aggregate roots own a collection of pending domain events that are
/// dispatched after the unit of work commits.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>Domain events raised during the current operation (not yet dispatched).</summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>Removes and returns all pending domain events.</summary>
    IReadOnlyList<IDomainEvent> PopDomainEvents();
}
