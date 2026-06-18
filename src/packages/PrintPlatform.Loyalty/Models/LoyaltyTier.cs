namespace PrintPlatform.Loyalty.Models;

/// <summary>
/// Defines a named loyalty tier with entry threshold, earning multiplier, and benefits.
/// Seeded at startup; can be overridden via host configuration.
/// </summary>
public sealed class LoyaltyTier
{
    /// <summary>Surrogate PK.</summary>
    public long Id { get; set; }

    /// <summary>Human-readable name (Bronze, Silver, Gold, Platinum …).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Minimum lifetime points required to enter (and stay in) this tier.</summary>
    public int MinPoints { get; set; }

    /// <summary>
    /// Points multiplier applied on top of base earning.
    /// e.g. 1.5 means 50 % extra points per order.
    /// </summary>
    public double Multiplier { get; set; } = 1.0;

    /// <summary>JSON document of tier benefits (e.g. discount %, feature flags).</summary>
    public string BenefitsJson { get; set; } = "{}";

    /// <summary>Ascending sort order for tier ranking (lower = lower tier).</summary>
    public int SortOrder { get; set; }

    // Navigation
    public ICollection<LoyaltyMember> Members { get; set; } = [];
}
