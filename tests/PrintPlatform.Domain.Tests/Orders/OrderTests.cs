using FluentAssertions;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;
using System;
using System.Linq;
using System.Reflection;

namespace PrintPlatform.Domain.Tests.Orders;

public sealed class OrderTests
{
    private static Order CreateTestOrder()
    {
        return Order.CreateFromAcceptedQuote(
            customerUserId: Guid.NewGuid(),
            quoteId: Guid.NewGuid(),
            quantity: 1,
            unitPriceEgp: 100m,
            modelFileStorageKey: "test-key",
            paymentMethod: PaymentMethod.Card,
            shippingAddressSnapshot: "{}",
            deliveryFeeEgp: 50m,
            discountEgp: 0m,
            loyaltyPointsUsed: 0,
            loyaltyDiscountEgp: 0m);
    }

    private static Order SetupOrderInStatus(OrderStatus targetStatus)
    {
        var order = CreateTestOrder();

        // Progress the order through its natural lifecycle to reach the target status
        switch (targetStatus)
        {
            case OrderStatus.PendingPayment:
                break; // Default state
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.InProduction:
                order.Confirm();
                order.MoveToProduction();
                break;
            case OrderStatus.QCPending:
                order.Confirm();
                order.MoveToProduction();
                order.MoveToQc();
                break;
            case OrderStatus.ReadyToShip:
                order.Confirm();
                order.MoveToProduction();
                order.MoveToQc();
                order.MarkReadyToShip();
                break;
            case OrderStatus.Shipped:
                order.Confirm();
                order.MoveToProduction();
                order.MoveToQc();
                order.MarkReadyToShip();
                order.MarkShipped();
                break;
            case OrderStatus.Delivered:
                order.Confirm();
                order.MoveToProduction();
                order.MoveToQc();
                order.MarkReadyToShip();
                order.MarkShipped();
                order.MarkDelivered();
                break;
            case OrderStatus.Cancelled:
                order.Cancel("Cancellation during setup");
                break;
            case OrderStatus.Refunded:
                // OrderStatus.Refunded cannot be reached via public transition methods in Order.cs.
                // It seems to be missing a transition or relies on downstream modification.
                // We use reflection here ONLY to test the disallowed state transition logic.
                var statusProperty = typeof(Order).GetProperty(nameof(Order.Status));
                statusProperty?.SetValue(order, OrderStatus.Refunded);
                break;
        }

        return order;
    }

    [Fact]
    public void Cancel_WhenSuccessful_UpdatesStatusToCancelled_AndRaisesEvent()
    {
        var order = CreateTestOrder();
        order.PopDomainEvents(); // Clear any initial events
        string reason = "Customer requested cancellation";

        var result = order.Cancel(reason);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);

        var events = order.PopDomainEvents();
        events.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCancelledEvent>()
            .Which.Reason.Should().Be(reason);
    }

    [Fact]
    public void Cancel_WhenPaymentIsPaid_ChangesPaymentStatusToRefunded()
    {
        var order = CreateTestOrder();
        order.Confirm(); // Sets payment to Paid
        order.PaymentStatus.Should().Be(PaymentStatus.Paid);
        order.PopDomainEvents();

        var result = order.Cancel("Cancel for refund");

        result.IsSuccess.Should().BeTrue();
        order.PaymentStatus.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void Cancel_WhenPaymentIsPending_KeepsPaymentStatusAsPending()
    {
        var order = CreateTestOrder();
        order.PaymentStatus.Should().Be(PaymentStatus.Pending);
        order.PopDomainEvents();

        var result = order.Cancel("Cancel before paying");

        result.IsSuccess.Should().BeTrue();
        order.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void Cancel_WhenSuccessful_UpdatesUpdatedAt()
    {
        var order = CreateTestOrder();
        var before = DateTimeOffset.UtcNow;

        var result = order.Cancel("Reason");

        var after = DateTimeOffset.UtcNow;

        result.IsSuccess.Should().BeTrue();
        order.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Theory]
    [InlineData(OrderStatus.PendingPayment)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.InProduction)]
    [InlineData(OrderStatus.QCPending)]
    [InlineData(OrderStatus.ReadyToShip)]
    public void Cancel_WhenStatusIsAllowed_Succeeds(OrderStatus status)
    {
        var order = SetupOrderInStatus(status);
        order.PopDomainEvents();

        var result = order.Cancel("Reason");

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Refunded)]
    public void Cancel_WhenStatusIsNotAllowed_ReturnsFailure_AndDoesNotChangeState(OrderStatus status)
    {
        var order = SetupOrderInStatus(status);
        order.PopDomainEvents();

        var initialUpdatedAt = order.UpdatedAt;
        var initialPaymentStatus = order.PaymentStatus;

        var result = order.Cancel("Reason");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.OrderNotCancellable);

        order.Status.Should().Be(status);
        order.PaymentStatus.Should().Be(initialPaymentStatus);
        order.UpdatedAt.Should().Be(initialUpdatedAt);
        order.DomainEvents.Should().BeEmpty();
    }
}
