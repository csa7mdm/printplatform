namespace PrintPlatform.Gamification.Models;

/// <summary>
/// Represents a participant in the gamification system.
/// Identified by <see cref="ExternalUserId"/> — a string opaque to this package.
/// </summary>
public sealed class GamificationPlayer
{
    /// <summary>Surrogate primary key.</summary>
    public long Id { get; set; }

    /// <summary>
    /// Identifier owned by the host platform (e.g. its User.Id as a string).
    /// Unique, never null, never changed after creation.
    /// </summary>
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>Display name shown on leaderboards and achievement feeds.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Aggregate XP total, recalculated on every <c>AwardPointsAsync</c> call.</summary>
    public int TotalXP { get; set; }

    /// <summary>Current level tier (matches <see cref="LevelDefinition.Tier"/>).</summary>
    public int CurrentLevel { get; set; } = 1;

    /// <summary>ISO-8601 timestamp of account creation in the gamification system.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Last time any XP was awarded to this player.</summary>
    public DateTimeOffset? LastActivityAt { get; set; }

    // Navigation
    public ICollection<PointTransaction> Transactions { get; set; } = [];
    public ICollection<PlayerAchievement> Achievements  { get; set; } = [];
    public ICollection<PlayerChallenge>   Challenges    { get; set; } = [];
    public ICollection<Streak>            Streaks       { get; set; } = [];
}
