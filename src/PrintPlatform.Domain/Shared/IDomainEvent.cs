namespace PrintPlatform.Domain.Shared;

/// <summary>
/// Marker interface for all domain events.
/// Domain events are plain records — no MediatR coupling here.
/// The Application layer wraps them in <c>DomainEventNotification&lt;T&gt;</c>
/// before dispatching via MediatR.
/// </summary>
public interface IDomainEvent
{
    /// <summary>When the event occurred (UTC).</summary>
    DateTimeOffset OccurredAt { get; }
}
