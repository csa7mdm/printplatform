namespace PrintPlatform.Gamification.Models;

/// <summary>
/// Junction record written when a player unlocks an <see cref="Achievement"/>.
/// One row per player/achievement pair; re-unlocking is not supported.
/// </summary>
public sealed class PlayerAchievement
{
    public long PlayerId { get; set; }
    public long AchievementId { get; set; }

    /// <summary>UTC timestamp at which the achievement was first unlocked.</summary>
    public DateTimeOffset UnlockedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public GamificationPlayer Player      { get; set; } = null!;
    public Achievement         Achievement { get; set; } = null!;
}
