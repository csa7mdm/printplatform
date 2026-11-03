using FluentAssertions;
using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Domain.Tests.Orders;

public class OrderTests
{
    [Fact]
    public void CreateFromAcceptedQuote_Should_InitializeOrderCorrectly()
    {
        // Arrange
        var customerUserId = Guid.NewGuid();
        var quoteId = Guid.NewGuid();
        var quantity = 2;
        var unitPrice = 150.5m;
        var storageKey = "models/file.stl";
        var paymentMethod = PaymentMethod.Card;
        var addressSnapshot = "{\"city\":\"Cairo\",\"street\":\"Talaat Harb\"}";
        var deliveryFee = 50m;
        var discount = 10m;
        var loyaltyPointsUsed = 100L;
        var loyaltyDiscount = 20m;

        // Act
        var order = Order.CreateFromAcceptedQuote(
            customerUserId,
            quoteId,
            quantity,
            unitPrice,
            storageKey,
            paymentMethod,
            addressSnapshot,
            deliveryFee,
            discount,
            loyaltyPointsUsed,
            loyaltyDiscount);

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().NotBeEmpty();
        order.CustomerUserId.Should().Be(customerUserId);
        order.Status.Should().Be(OrderStatus.PendingPayment);
        order.PaymentMethod.Should().Be(paymentMethod);
        order.PaymentStatus.Should().Be(PaymentStatus.Pending);
        order.ShippingAddressSnapshot.Should().Be(addressSnapshot);
        order.DeliveryFeeEgp.Should().Be(deliveryFee);
        order.DiscountEgp.Should().Be(discount);
        order.LoyaltyPointsUsed.Should().Be(loyaltyPointsUsed);
        order.LoyaltyDiscountEgp.Should().Be(loyaltyDiscount);
    }

    [Fact]
    public void CreateFromAcceptedQuote_Should_CreateSingleOrderItem()
    {
        // Arrange
        var customerUserId = Guid.NewGuid();
        var quoteId = Guid.NewGuid();
        var quantity = 3;
        var unitPrice = 100m;
        var storageKey = "models/test.stl";
        var paymentMethod = PaymentMethod.COD;
        var addressSnapshot = "snapshot";
        var deliveryFee = 20m;
        var discount = 0m;
        var loyaltyPointsUsed = 0L;
        var loyaltyDiscount = 0m;

        // Act
        var order = Order.CreateFromAcceptedQuote(
            customerUserId,
            quoteId,
            quantity,
            unitPrice,
            storageKey,
            paymentMethod,
            addressSnapshot,
            deliveryFee,
            discount,
            loyaltyPointsUsed,
            loyaltyDiscount);

        // Assert
        order.Items.Should().ContainSingle();

        var item = order.Items.Single();
        item.Id.Should().NotBeEmpty();
        item.OrderId.Should().Be(order.Id);
        item.QuoteId.Should().Be(quoteId);
        item.Quantity.Should().Be(quantity);
        item.UnitPriceEgp.Should().Be(unitPrice);
        item.ModelFileStorageKey.Should().Be(storageKey);

        // Quantity * UnitPriceEgp
        item.TotalEgp.Should().Be(quantity * unitPrice);
    }

    [Fact]
    public void CreateFromAcceptedQuote_Should_CalculateTotalsCorrectly()
    {
        // Arrange
        var customerUserId = Guid.NewGuid();
        var quoteId = Guid.NewGuid();
        var quantity = 2;
        var unitPrice = 150m; // Item total = 300
        var storageKey = "models/obj.obj";
        var paymentMethod = PaymentMethod.InstaPay;
        var addressSnapshot = "Alexandria";
        var deliveryFee = 50m;
        var discount = 20m;
        var loyaltyPointsUsed = 500L;
        var loyaltyDiscount = 50m;

        // Expected Math:
        // Subtotal = 300
        // Total = 300 (Subtotal) + 50 (Delivery) - 20 (Discount) - 50 (Loyalty) = 280

        // Act
        var order = Order.CreateFromAcceptedQuote(
            customerUserId,
            quoteId,
            quantity,
            unitPrice,
            storageKey,
            paymentMethod,
            addressSnapshot,
            deliveryFee,
            discount,
            loyaltyPointsUsed,
            loyaltyDiscount);

        // Assert
        order.SubtotalEgp.Should().Be(300m);
        order.TotalEgp.Should().Be(280m);
    }
}
