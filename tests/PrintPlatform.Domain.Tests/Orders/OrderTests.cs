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
            "test-model.stl",
            PaymentMethod.Card,
            "{}",
            50m,
            10m,
            100,
            20m);
    }

    [Fact]
    public void Cancel_PendingPayment_Order_ShouldCancelAndRaiseEvent()
    {
        // Arrange
        var order = CreateTestOrder();
        var customerId = order.CustomerUserId;
        var reason = "User requested cancellation before payment";

        // Act
        var result = order.Cancel(reason);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.PaymentStatus.Should().Be(PaymentStatus.Pending); // Still Pending

        var domainEvents = order.PopDomainEvents();
        var cancelledEvent = domainEvents.OfType<OrderCancelledEvent>().SingleOrDefault();
        cancelledEvent.Should().NotBeNull();
        cancelledEvent!.OrderId.Should().Be(order.Id);
        cancelledEvent.CustomerUserId.Should().Be(customerId);
        cancelledEvent.Reason.Should().Be(reason);
    }

    [Fact]
    public void Cancel_ConfirmedOrder_ShouldCancelAndMarkPaymentAsRefunded()
    {
        // Arrange
        var order = CreateTestOrder();
        order.Confirm(); // Moves PaymentStatus to Paid, Status to Confirmed

        // Clear events from Confirm
        order.PopDomainEvents();

        var reason = "Changed my mind after payment";

        // Act
        var result = order.Cancel(reason);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.PaymentStatus.Should().Be(PaymentStatus.Refunded);

        var domainEvents = order.PopDomainEvents();
        var cancelledEvent = domainEvents.OfType<OrderCancelledEvent>().SingleOrDefault();
        cancelledEvent.Should().NotBeNull();
        cancelledEvent!.Reason.Should().Be(reason);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Refunded)]
    public void Cancel_WhenOrderIsNotCancellable_ShouldReturnFailure(OrderStatus targetStatus)
    {
        // Arrange
        var order = CreateTestOrder();

        // Progress order to the target status
        ProgressOrderToStatus(order, targetStatus);

        // Clear previous events
        order.PopDomainEvents();

        // Act
        var result = order.Cancel("Too late");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(OrderErrors.OrderNotCancellable.Code);
        order.Status.Should().Be(targetStatus); // Status should remain unchanged

        var domainEvents = order.PopDomainEvents();
        domainEvents.OfType<OrderCancelledEvent>().Should().BeEmpty();
    }

    private static void ProgressOrderToStatus(Order order, OrderStatus targetStatus)
    {
        if (targetStatus == OrderStatus.PendingPayment) return;

        if (targetStatus == OrderStatus.Cancelled)
        {
            order.Cancel("Early cancel");
            return;
        }

        // Move to Confirmed
        order.Confirm();
        if (targetStatus == OrderStatus.Confirmed) return;

        // Move to InProduction
        order.MoveToProduction();
        if (targetStatus == OrderStatus.InProduction) return;

        // Move to QCPending
        order.MoveToQc();
        if (targetStatus == OrderStatus.QCPending) return;

        // Move to ReadyToShip
        order.MarkReadyToShip();
        if (targetStatus == OrderStatus.ReadyToShip) return;

        // Move to Shipped
        order.MarkShipped();
        if (targetStatus == OrderStatus.Shipped) return;

        // Move to Delivered
        if (targetStatus == OrderStatus.Delivered)
        {
            order.MarkDelivered();
            return;
        }

        // Since refunding happens on an event listener elsewhere, we can force a cancel if it's refunded,
        // but wait, there is no direct Refunded transition on Order itself besides Cancel setting PaymentStatus.
        // If targetStatus is Refunded, we might need a workaround because OrderStatus doesn't have a direct Refunded state without reflection if it's not possible to reach it normally.
        // Actually, looking at Order.cs:
        // public enum OrderStatus { PendingPayment, Confirmed, InProduction, QCPending, ReadyToShip, Shipped, Delivered, Cancelled, Refunded }
        // Wait, how does it become Refunded? There is no transition to Refunded in Order.cs.
        // Let's use reflection to force the state for the test just in case.

        if (targetStatus == OrderStatus.Refunded)
        {
            var property = typeof(Order).GetProperty(nameof(Order.Status));
            property!.SetValue(order, OrderStatus.Refunded);
        }
    }
}
