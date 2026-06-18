namespace PrintPlatform.Loyalty.Models;

/// <summary>
/// A reward that members can redeem using accumulated points.
/// </summary>
public sealed class Reward
{
    /// <summary>Surrogate PK.</summary>
    public long Id { get; set; }

    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Points cost to redeem this reward.</summary>
    public int PointsCost { get; set; }

    /// <summary>Functional category of the reward.</summary>
    public RewardType Type { get; set; }

    /// <summary>
    /// Monetary value of the reward in EGP.
    /// For Discount: percentage (e.g. 10 = 10%).
    /// For Credit:   flat EGP amount.
    /// For FreeService: platform-defined service value.
    /// </summary>
    public double Value { get; set; }

    /// <summary>Whether the reward is currently available for redemption.</summary>
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<Redemption> Redemptions { get; set; } = [];
}

/// <summary>Category of reward benefit.</summary>
public enum RewardType
{
    /// <summary>Percentage discount applied to a future order.</summary>
    Discount = 1,

    /// <summary>A specific service provided at no charge (e.g. design review).</summary>
    FreeService = 2,

    /// <summary>Flat EGP credit added to the member's account.</summary>
    Credit = 3,
}
