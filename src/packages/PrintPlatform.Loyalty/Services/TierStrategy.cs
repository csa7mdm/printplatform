using PrintPlatform.Loyalty.Abstractions;
using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Services;

public class TierStrategy : ITierStrategy
{
    public LoyaltyTier Evaluate(int lifetimePoints, IReadOnlyList<LoyaltyTier> allTiers)
    {
        if (allTiers == null || !allTiers.Any())
            throw new InvalidOperationException("No loyalty tiers configured.");

        return allTiers
            .OrderByDescending(t => t.MinPoints)
            .FirstOrDefault(t => lifetimePoints >= t.MinPoints)
            ?? allTiers.OrderBy(t => t.MinPoints).First();
    }
}
