namespace PrintPlatform.Gamification.Models;

/// <summary>
/// One player's score entry within a <see cref="Leaderboard"/>.
/// Upserted by <c>LeaderboardService</c> after each XP award.
/// </summary>
public sealed class LeaderboardEntry
{
    public long LeaderboardId { get; set; }
    public long PlayerId      { get; set; }

    /// <summary>Current score (meaning depends on <see cref="Leaderboard.Type"/>).</summary>
    public double Score { get; set; }

    /// <summary>1-based rank position; 1 = highest score.</summary>
    public int Rank { get; set; }

    /// <summary>UTC timestamp of the last score update.</summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Leaderboard        Leaderboard { get; set; } = null!;
    public GamificationPlayer Player      { get; set; } = null!;
}
