using MediatR;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Events;

/// <summary>
/// Implements <see cref="IDomainEventDispatcher"/> using MediatR's in-process
/// publish/subscribe bus.
/// Each domain event is wrapped in a <see cref="DomainEventNotification{T}"/> and
/// published so that any registered <see cref="INotificationHandler{TNotification}"/>
/// can react to it.
/// </summary>
public sealed class InternalEventBus : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;

    public InternalEventBus(IPublisher publisher) => _publisher = publisher;

    /// <inheritdoc />
    public async Task DispatchAndClearAsync(
        IEnumerable<IAggregateRoot> aggregates,
        CancellationToken cancellationToken = default)
    {
        // Collect and clear in one pass to avoid double-dispatch if this is called twice.
        var events = aggregates
            .SelectMany(a => a.PopDomainEvents())
            .ToList();

        foreach (var domainEvent in events)
        {
            // Build DomainEventNotification<T> via reflection so the generic type
            // parameter matches the concrete event type, allowing typed handlers.
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
            await _publisher.Publish(notification, cancellationToken);
        }
    }
}
