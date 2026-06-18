using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Orders;

/// <summary>
/// A single line on an <see cref="Order"/>, snapshotted from an accepted <see cref="Quote"/>.
/// Stores the model file's storage key as a point-in-time snapshot so production can
/// always locate the exact geometry that was priced.
/// </summary>
public sealed class OrderItem : BaseEntity<Guid>
{
    private OrderItem() { } // EF

    internal OrderItem(
        Guid orderId,
        Guid quoteId,
        int quantity,
        decimal unitPriceEgp,
        string modelFileStorageKey)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        QuoteId = quoteId;
        Quantity = quantity;
        UnitPriceEgp = unitPriceEgp;
        TotalEgp = unitPriceEgp * quantity;
        ModelFileStorageKey = modelFileStorageKey;
    }

    public Guid OrderId { get; private set; }
    public Guid QuoteId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPriceEgp { get; private set; }
    public decimal TotalEgp { get; private set; }

    /// <summary>Snapshot of the model file's object-storage key at order time.</summary>
    public string ModelFileStorageKey { get; private set; } = default!;
}
