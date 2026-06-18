using MediatR;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Events;

/// <summary>
/// Wraps a <see cref="IDomainEvent"/> as a MediatR <see cref="INotification"/>
/// so it can be dispatched through the in-process event bus.
/// </summary>
/// <typeparam name="TDomainEvent">The concrete domain event type.</typeparam>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent)
    : INotification
    where TDomainEvent : IDomainEvent;
