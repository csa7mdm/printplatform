namespace PrintPlatform.Loyalty.Models;

/// <summary>
/// Immutable ledger entry representing a single points movement.
/// The table is APPEND-ONLY — rows are never updated or deleted.
/// Positive <see cref="Delta"/> = earned/bonus; negative = redeemed/expired.
/// </summary>
public sealed class PointsLedger
{
    /// <summary>Surrogate PK (identity / auto-increment).</summary>
    public long Id { get; set; }

    /// <summary>FK to <see cref="LoyaltyMember"/>.</summary>
    public long MemberId { get; set; }

    /// <summary>Navigation property.</summary>
    public LoyaltyMember Member { get; set; } = null!;

    /// <summary>
    /// Signed points movement. Positive for Earned/Bonus; negative for Redeemed/Expired.
    /// </summary>
    public int Delta { get; set; }

    /// <summary>Semantic type of this movement.</summary>
    public LedgerEntryType Type { get; set; }

    /// <summary>
    /// Optional reference to the host-platform order that triggered this movement.
    /// </summary>
    public string? OrderReference { get; set; }

    /// <summary>
    /// UTC timestamp after which this credit is no longer spendable.
    /// Null for debit rows (Redeemed/Expired) — they have no expiry.
    /// </summary>
    public DateTimeOffset? Expiry { get; set; }

    /// <summary>UTC timestamp the row was inserted.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>Semantic category of a points ledger entry.</summary>
public enum LedgerEntryType
{
    /// <summary>Points earned through an order (base + multipliers).</summary>
    Earned = 1,

    /// <summary>Points spent to redeem a reward.</summary>
    Redeemed = 2,

    /// <summary>Credits that have passed their expiry date, zeroed out by the expiry job.</summary>
    Expired = 3,

    /// <summary>Manual or system bonus (first order, referral, birthday, admin grant).</summary>
    Bonus = 4,
}
