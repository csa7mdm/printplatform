namespace PrintPlatform.Gamification.Models;

/// <summary>
/// Tracks consecutive occurrences of a specific action type for a player.
/// One row per player/actionType pair. Reset to 0 when the gap between consecutive
/// events exceeds <see cref="GamificationOptions.StreakBreakThresholdHours"/>.
/// </summary>
public sealed class Streak
{
    public long Id { get; set; }

    public long PlayerId { get; set; }

    /// <summary>The action string this streak tracks (e.g. "QCApproved").</summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>Number of consecutive occurrences without a break.</summary>
    public int CurrentCount { get; set; }

    /// <summary>All-time maximum streak count achieved.</summary>
    public int BestCount { get; set; }

    /// <summary>UTC timestamp of the most recent qualifying action.</summary>
    public DateTimeOffset LastActionDate { get; set; }

    // Navigation
    public GamificationPlayer Player { get; set; } = null!;
}
