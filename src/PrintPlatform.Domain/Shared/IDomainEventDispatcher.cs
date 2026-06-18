namespace PrintPlatform.Domain.Shared;

/// <summary>
/// Abstracts the dispatch mechanism so the Domain layer stays clean.
/// Implemented in the Application layer using MediatR.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches all pending domain events collected from the given aggregate roots.
    /// Call this inside <c>SaveChangesAsync</c> after the DB commit.
    /// </summary>
    Task DispatchAndClearAsync(
        IEnumerable<IAggregateRoot> aggregates,
        CancellationToken cancellationToken = default);
}
