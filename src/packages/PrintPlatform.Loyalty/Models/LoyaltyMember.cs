namespace PrintPlatform.Loyalty.Models;

/// <summary>
/// A member enrolled in the loyalty programme.
/// Identified by <see cref="ExternalUserId"/> — opaque to this package.
/// </summary>
public sealed class LoyaltyMember
{
    /// <summary>Surrogate PK.</summary>
    public long Id { get; set; }

    /// <summary>
    /// Opaque identifier from the host platform (e.g. User.Id.ToString()).
    /// Never changed after creation.
    /// </summary>
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>FK to <see cref="LoyaltyTier"/>. Null until first earn.</summary>
    public long? CurrentTierId { get; set; }

    /// <summary>Navigation to the current tier (may be null before first earn).</summary>
    public LoyaltyTier? CurrentTier { get; set; }

    /// <summary>
    /// Sum of all positive ledger entries ever — never decremented.
    /// Used for tier evaluation (lifetime progress).
    /// </summary>
    public int LifetimePoints { get; set; }

    /// <summary>
    /// Current spendable balance: sum of all non-expired ledger deltas.
    /// Recalculated after every earn/redeem/expiry operation.
    /// </summary>
    public int ActivePoints { get; set; }

    /// <summary>Optional: member's date-of-birth for birthday multiplier.</summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>Number of completed orders — used to detect first order.</summary>
    public int CompletedOrderCount { get; set; }

    /// <summary>Referral code unique to this member (generated on enrolment).</summary>
    public string ReferralCode { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigations
    public ICollection<PointsLedger> LedgerEntries { get; set; } = [];
    public ICollection<Redemption>   Redemptions   { get; set; } = [];
}
