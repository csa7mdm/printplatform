using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Abstractions;

/// <summary>
/// Pluggable tier-evaluation strategy.
/// Replace the default implementation via DI to apply custom tier logic
/// (e.g. tier based on order count rather than lifetime points).
/// </summary>
public interface ITierStrategy
{
    /// <summary>
    /// Determines the correct <see cref="LoyaltyTier"/> for the given
    /// <paramref name="lifetimePoints"/>, selecting from <paramref name="allTiers"/>.
    /// </summary>
    /// <param name="lifetimePoints">Member's current cumulative earned points (never negative).</param>
    /// <param name="allTiers">All configured tiers, in any order.</param>
    /// <returns>The applicable tier; must never return <c>null</c> (lowest tier is the fallback).</returns>
    LoyaltyTier Evaluate(int lifetimePoints, IReadOnlyList<LoyaltyTier> allTiers);
}
