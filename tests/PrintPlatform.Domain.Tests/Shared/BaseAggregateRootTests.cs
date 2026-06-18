using FluentAssertions;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Tests.Shared;

public sealed class BaseAggregateRootTests
{
    // Minimal concrete aggregate root for testing
    private sealed class FakeAggregate : BaseAggregateRoot<Guid>
    {
        public FakeAggregate() => Id = Guid.NewGuid();

        public void DoSomething() =>
            RaiseDomainEvent(new FakeEvent(DateTimeOffset.UtcNow));
    }

    private sealed record FakeEvent(DateTimeOffset OccurredAt) : IDomainEvent;

    [Fact]
    public void RaiseDomainEvent_adds_to_DomainEvents()
    {
        var agg = new FakeAggregate();
        agg.DoSomething();
        agg.DomainEvents.Should().HaveCount(1)
           .And.AllBeOfType<FakeEvent>();
    }

    [Fact]
    public void PopDomainEvents_clears_list_and_returns_events()
    {
        var agg = new FakeAggregate();
        agg.DoSomething();
        agg.DoSomething();

        var events = agg.PopDomainEvents();

        events.Should().HaveCount(2);
        agg.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SoftDelete_sets_IsDeleted_true()
    {
        var agg = new FakeAggregate();
        agg.SoftDelete();
        agg.IsDeleted.Should().BeTrue();
    }
}
