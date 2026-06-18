using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Orders;

/// <summary>
/// The order aggregate. Created in <see cref="OrderStatus.PendingPayment"/> when a
/// customer accepts a quote, then progresses through payment, production, QC, and delivery.
/// Owns its <see cref="OrderItem"/> lines and is the consistency boundary for status changes.
/// </summary>
public sealed class Order : BaseAggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = [];

    private Order() { } // EF

    private Order(
        Guid id,
        Guid customerUserId,
        PaymentMethod paymentMethod,
        string shippingAddressSnapshot,
        decimal deliveryFeeEgp,
        decimal discountEgp,
        long loyaltyPointsUsed,
        decimal loyaltyDiscountEgp)
    {
        Id = id;
        CustomerUserId = customerUserId;
        Status = OrderStatus.PendingPayment;
        PaymentMethod = paymentMethod;
        PaymentStatus = PaymentStatus.Pending;
        ShippingAddressSnapshot = shippingAddressSnapshot;
        DeliveryFeeEgp = deliveryFeeEgp;
        DiscountEgp = discountEgp;
        LoyaltyPointsUsed = loyaltyPointsUsed;
        LoyaltyDiscountEgp = loyaltyDiscountEgp;
    }

    public Guid CustomerUserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }

    /// <summary>Paymob order id returned by the gateway when payment is registered.</summary>
    public string? PaymobOrderId { get; private set; }

    /// <summary>JSON snapshot of the shipping address at order time.</summary>
    public string ShippingAddressSnapshot { get; private set; } = default!;

    public decimal SubtotalEgp { get; private set; }
    public decimal DeliveryFeeEgp { get; private set; }
    public decimal DiscountEgp { get; private set; }
    public decimal TotalEgp { get; private set; }

    public long LoyaltyPointsUsed { get; private set; }
    public decimal LoyaltyDiscountEgp { get; private set; }

    /// <summary>Optimistic-concurrency token (xmin / rowversion).</summary>
    public uint RowVersion { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Factory: creates a PendingPayment order from an accepted quote. Computes the
    /// subtotal from the line and the total (subtotal + delivery − discount − loyalty).
    /// </summary>
    public static Order CreateFromAcceptedQuote(
        Guid customerUserId,
        Guid quoteId,
        int quantity,
        decimal unitPriceEgp,
        string modelFileStorageKey,
        PaymentMethod paymentMethod,
        string shippingAddressSnapshot,
        decimal deliveryFeeEgp,
        decimal discountEgp,
        long loyaltyPointsUsed,
        decimal loyaltyDiscountEgp)
    {
        var order = new Order(
            Guid.NewGuid(), customerUserId, paymentMethod, shippingAddressSnapshot,
            deliveryFeeEgp, discountEgp, loyaltyPointsUsed, loyaltyDiscountEgp);

        var item = new OrderItem(order.Id, quoteId, quantity, unitPriceEgp, modelFileStorageKey);
        order._items.Add(item);

        order.SubtotalEgp = item.TotalEgp;
        order.RecalculateTotal();
        return order;
    }

    private void RecalculateTotal() =>
        TotalEgp = SubtotalEgp + DeliveryFeeEgp - DiscountEgp - LoyaltyDiscountEgp;

    /// <summary>Stores the Paymob gateway order id once a payment intent has been registered.</summary>
    public void AttachPaymobOrder(string paymobOrderId)
    {
        PaymobOrderId = paymobOrderId;
        Touch();
    }

    /// <summary>
    /// Confirms the order after successful payment capture. Idempotent: a second call
    /// once already Confirmed (or beyond) is a no-op success.
    /// </summary>
    public Result Confirm()
    {
        if (Status == OrderStatus.PendingPayment)
        {
            Status = OrderStatus.Confirmed;
            PaymentStatus = PaymentStatus.Paid;
            Touch();
            RaiseDomainEvent(new OrderConfirmedEvent(Id, CustomerUserId, TotalEgp, DateTimeOffset.UtcNow));
            return Result.Success();
        }

        // Idempotency: already confirmed/in-flight is acceptable for webhook replays.
        if (Status >= OrderStatus.Confirmed && Status <= OrderStatus.Delivered)
            return Result.Success();

        return Result.Failure(OrderErrors.OrderNotPendingPayment);
    }

    public Result MoveToProduction() => Transition(OrderStatus.Confirmed, OrderStatus.InProduction);
    public Result MoveToQc() => Transition(OrderStatus.InProduction, OrderStatus.QCPending);
    public Result MarkReadyToShip() => Transition(OrderStatus.QCPending, OrderStatus.ReadyToShip);
    public Result MarkShipped() => Transition(OrderStatus.ReadyToShip, OrderStatus.Shipped);

    public Result MarkDelivered()
    {
        var result = Transition(OrderStatus.Shipped, OrderStatus.Delivered);
        if (result.IsSuccess)
            RaiseDomainEvent(new OrderDeliveredEvent(Id, CustomerUserId, DateTimeOffset.UtcNow));
        return result;
    }

    /// <summary>
    /// Cancels the order if it has not yet shipped. Refund handling is delegated to the
    /// finance module via downstream reaction to <see cref="OrderCancelledEvent"/>.
    /// </summary>
    public Result Cancel(string reason)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered
            or OrderStatus.Cancelled or OrderStatus.Refunded)
            return Result.Failure(OrderErrors.OrderNotCancellable);

        Status = OrderStatus.Cancelled;
        if (PaymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyPaid)
            PaymentStatus = PaymentStatus.Refunded;
        Touch();
        RaiseDomainEvent(new OrderCancelledEvent(Id, CustomerUserId, reason, DateTimeOffset.UtcNow));
        return Result.Success();
    }

    private Result Transition(OrderStatus expected, OrderStatus next)
    {
        if (Status != expected)
            return Result.Failure(OrderErrors.InvalidStatusTransition(Status, next));
        Status = next;
        Touch();
        return Result.Success();
    }
}
