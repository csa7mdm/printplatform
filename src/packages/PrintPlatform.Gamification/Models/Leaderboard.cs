namespace PrintPlatform.Gamification.Models;

/// <summary>Defines a leaderboard slot. Entries are stored in <see cref="LeaderboardEntry"/>.</summary>
public sealed class Leaderboard
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Time window: <c>Daily</c>, <c>Weekly</c>, or <c>AllTime</c>.</summary>
    public LeaderboardPeriod Period { get; set; }

    /// <summary>Score metric tracked by this leaderboard.</summary>
    public LeaderboardType Type { get; set; }

    public ICollection<LeaderboardEntry> Entries { get; set; } = [];
}

/// <summary>Time window for a leaderboard.</summary>
public enum LeaderboardPeriod { Daily, Weekly, AllTime }

/// <summary>Metric used as the score on a leaderboard.</summary>
public enum LeaderboardType { XP, JobCount, Rating }
