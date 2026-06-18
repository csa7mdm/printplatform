namespace PrintPlatform.Loyalty;

/// <summary>
/// Strongly-typed options for the Loyalty module.
/// Bound from <c>appsettings.json → "Loyalty"</c>.
/// All thresholds/rates are overridable at host startup.
/// </summary>
public sealed class LoyaltyOptions
{
    public const string SectionName = "Loyalty";

    // ── Tier thresholds (lifetime points) ─────────────────────────────────────
    /// <summary>Silver starts at 1 000 lifetime points.</summary>
    public int SilverThreshold { get; set; } = 1_000;

    /// <summary>Gold starts at 5 000 lifetime points.</summary>
    public int GoldThreshold { get; set; } = 5_000;

    /// <summary>Platinum starts at 15 000 lifetime points.</summary>
    public int PlatinumThreshold { get; set; } = 15_000;

    // ── Tier multipliers ───────────────────────────────────────────────────────
    public double BronzeMultiplier   { get; set; } = 1.0;
    public double SilverMultiplier   { get; set; } = 1.2;
    public double GoldMultiplier     { get; set; } = 1.5;
    public double PlatinumMultiplier { get; set; } = 2.0;

    // ── Earning rates ──────────────────────────────────────────────────────────
    /// <summary>Base points per 1 EGP spent (default: 0.1 → 1 pt / 10 EGP).</summary>
    public double PointsPerEgp { get; set; } = 0.1;

    /// <summary>Extra multiplier applied when a Design service is attached to the order.</summary>
    public double DesignServiceMultiplier { get; set; } = 2.0;

    /// <summary>Extra multiplier applied during the member's birthday month.</summary>
    public double BirthdayMultiplier { get; set; } = 2.0;

    /// <summary>Flat bonus awarded on a member's very first order.</summary>
    public int FirstOrderBonusPoints { get; set; } = 200;

    /// <summary>Bonus awarded to the referrer after the referee's first order completes.</summary>
    public int ReferralBonusPoints { get; set; } = 500;

    // ── Expiry ─────────────────────────────────────────────────────────────────
    /// <summary>Points expire after this many months of inactivity (0 = never).</summary>
    public int ExpiryMonths { get; set; } = 12;

    // ── Redemption ────────────────────────────────────────────────────────────
    /// <summary>Monetary value (EGP) of one redeemable point.</summary>
    public double PointValueEgp { get; set; } = 0.5;
}
