namespace PrintPlatform.Loyalty.Models;

/// <summary>
/// Records that a member redeemed a reward, consuming points from their balance.
/// Append-only — rows are never updated or deleted.
/// </summary>
public sealed class Redemption
{
    /// <summary>Surrogate PK.</summary>
    public long Id { get; set; }

    /// <summary>FK to <see cref="LoyaltyMember"/>.</summary>
    public long MemberId { get; set; }

    public LoyaltyMember Member { get; set; } = null!;

    /// <summary>FK to <see cref="Reward"/>.</summary>
    public long RewardId { get; set; }

    public Reward Reward { get; set; } = null!;

    /// <summary>Points deducted at time of redemption (snapshot of reward.PointsCost).</summary>
    public int PointsUsed { get; set; }

    /// <summary>Optional host-platform order this redemption was applied to.</summary>
    public string? OrderReference { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
