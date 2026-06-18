namespace PrintPlatform.Gamification.Models;

/// <summary>
/// Persisted level snapshot seeded from <see cref="GamificationOptions.Levels"/>.
/// The engine resolves the current level from options at runtime; this table is
/// available for reporting queries and future history tracking.
/// </summary>
public sealed class Level
{
    /// <summary>Tier number (1 = lowest). Also the primary key.</summary>
    public int Tier { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Minimum XP required to reach this tier (inclusive).</summary>
    public int MinXP { get; set; }

    /// <summary>Maximum XP for this tier (exclusive upper bound; use <c>int.MaxValue</c> for the top tier).</summary>
    public int MaxXP { get; set; }

    /// <summary>JSON document describing perks unlocked at this tier.</summary>
    public string BenefitsJson { get; set; } = "{}";
}
