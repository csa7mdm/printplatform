using MediatR;
using NSubstitute;
using PrintPlatform.Application.Events;
using PrintPlatform.Domain.Shared;
using FluentAssertions;
using Xunit;

namespace PrintPlatform.Application.Tests.Events;

public sealed class InternalEventBusTests
{
    private sealed record OrderCreated(DateTimeOffset OccurredAt) : IDomainEvent;

    private sealed class StubAggregate : BaseAggregateRoot<Guid>
    {
        public StubAggregate() => Id = Guid.NewGuid();
        public void Create() => RaiseDomainEvent(new OrderCreated(DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task DispatchAndClearAsync_publishes_one_notification_per_event()
    {
        var publisher = Substitute.For<IPublisher>();
        var bus       = new InternalEventBus(publisher);

        var agg = new StubAggregate();
        agg.Create();
        agg.Create();

        await bus.DispatchAndClearAsync([agg]);

        await publisher.Received(2).Publish(
            Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchAndClearAsync_empties_aggregate_events()
    {
        var publisher = Substitute.For<IPublisher>();
        var bus       = new InternalEventBus(publisher);

        var agg = new StubAggregate();
        agg.Create();

        await bus.DispatchAndClearAsync([agg]);

        agg.DomainEvents.Should().BeEmpty();
    }
}
