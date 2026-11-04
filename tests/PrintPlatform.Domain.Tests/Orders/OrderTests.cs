using System.Reflection;
using FluentAssertions;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Tests.Orders;

public sealed class OrderTests
{
    private static Order CreateTestOrder()
    {
        return Order.CreateFromAcceptedQuote(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            100m,
            "path/to/model",
            PaymentMethod.Card,
            "{}",
            50m,
            10m,
            0,
            0m);
    }


    [Fact]
    public void Confirm_WhenPendingPayment_ShouldTransitionToConfirmedAndRaiseEvent()
    {
        // Arrange
        var order = CreateTestOrder();
        var initialEventCount = order.DomainEvents.Count;

        // Act
        var result = order.Confirm();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.PaymentStatus.Should().Be(PaymentStatus.Paid);

        var events = order.DomainEvents.ToList();
        events.Should().HaveCount(initialEventCount + 1);

        var confirmedEvent = events.OfType<OrderConfirmedEvent>().Single();
        confirmedEvent.OrderId.Should().Be(order.Id);
    }


    [Theory]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.InProduction)]
    [InlineData(OrderStatus.QCPending)]
    [InlineData(OrderStatus.ReadyToShip)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    public void Confirm_WhenAlreadyConfirmedOrBeyond_ShouldBeIdempotent(OrderStatus targetStatus)
    {
        // Arrange
        var order = CreateTestOrder();

        // Progress order to target status using domain transitions
        if (targetStatus >= OrderStatus.Confirmed) order.Confirm();
        if (targetStatus >= OrderStatus.InProduction) order.MoveToProduction();
        if (targetStatus >= OrderStatus.QCPending) order.MoveToQc();
        if (targetStatus >= OrderStatus.ReadyToShip) order.MarkReadyToShip();
        if (targetStatus >= OrderStatus.Shipped) order.MarkShipped();
        if (targetStatus >= OrderStatus.Delivered) order.MarkDelivered();

        // Ensure we reached the expected state setup
        order.Status.Should().Be(targetStatus);

        // Capture state before second Confirm
        var prePaymentStatus = order.PaymentStatus;

        // Capture initial domain events count
        var initialEventCount = order.DomainEvents.Count;

        // Act
        var result = order.Confirm();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(targetStatus);
        order.PaymentStatus.Should().Be(prePaymentStatus);
        order.DomainEvents.Count.Should().Be(initialEventCount);
    }


    [Theory]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Refunded)]
    public void Confirm_WhenInInvalidState_ShouldReturnFailure(OrderStatus targetStatus)
    {
        // Arrange
        var order = CreateTestOrder();

        if (targetStatus == OrderStatus.Cancelled)
        {
            order.Cancel("Test cancellation");
        }
        else if (targetStatus == OrderStatus.Refunded)
        {
            typeof(Order).GetProperty(nameof(Order.Status))!.SetValue(order, OrderStatus.Refunded);
        }

        // Ensure we reached the expected state setup
        order.Status.Should().Be(targetStatus);

        // Capture state before second Confirm
        var prePaymentStatus = order.PaymentStatus;

        // Capture initial domain events count
        var initialEventCount = order.DomainEvents.Count;

        // Act
        var result = order.Confirm();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(OrderErrors.OrderNotPendingPayment);

        order.Status.Should().Be(targetStatus);
        order.PaymentStatus.Should().Be(prePaymentStatus);
        order.DomainEvents.Count.Should().Be(initialEventCount);
    }


    [Fact]
    public void Confirm_WhenCalledRepeatedly_ShouldOnlyEmitOneEventAndChangeStateOnce()
    {
        // Arrange
        var order = CreateTestOrder();
        var initialEventCount = order.DomainEvents.Count;

        // Act - First call
        var firstResult = order.Confirm();

        // Assert - First call
        firstResult.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.PaymentStatus.Should().Be(PaymentStatus.Paid);

        var firstCallEvents = order.DomainEvents.ToList();
        firstCallEvents.Should().HaveCount(initialEventCount + 1);
        firstCallEvents.OfType<OrderConfirmedEvent>().Should().ContainSingle();

        // Capture state
        var statusAfterFirst = order.Status;
        var paymentStatusAfterFirst = order.PaymentStatus;
        var eventCountAfterFirst = order.DomainEvents.Count;

        // Act - Second call
        var secondResult = order.Confirm();

        // Assert - Second call
        secondResult.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(statusAfterFirst);
        order.PaymentStatus.Should().Be(paymentStatusAfterFirst);
        order.DomainEvents.Count.Should().Be(eventCountAfterFirst);
    }
}
