using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Abstractions;

/// <summary>
/// Pluggable reward-catalogue provider.
/// The default implementation reads from the <c>loy_rewards</c> table.
/// Replace with a remote catalogue adapter if rewards are managed externally.
/// </summary>
public interface IRewardProvider
{
    /// <summary>Returns all active rewards available for redemption.</summary>
    Task<IReadOnlyList<Reward>> GetActiveRewardsAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns a single reward by its identifier.
    /// Returns <c>null</c> if not found or no longer active.
    /// </summary>
    Task<Reward?> GetRewardAsync(long rewardId, CancellationToken ct = default);
}
