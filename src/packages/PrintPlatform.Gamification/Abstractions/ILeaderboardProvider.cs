using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Abstractions;

/// <summary>
/// Strategy for computing a player's score on a given leaderboard type.
/// Implement and register in DI to support custom scoring strategies
/// beyond the built-in XP / JobCount / Rating types.
/// </summary>
public interface ILeaderboardProvider
{
    /// <summary>The leaderboard type this provider handles.</summary>
    LeaderboardType SupportedType { get; }

    /// <summary>
    /// Computes the current score for <paramref name="player"/> to be stored in
    /// <see cref="LeaderboardEntry.Score"/>.
    /// </summary>
    /// <param name="player">Player snapshot (with loaded transactions if needed).</param>
    /// <param name="leaderboard">The leaderboard being updated (contains period/type info).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<double> ComputeScoreAsync(
        GamificationPlayer player,
        Leaderboard leaderboard,
        CancellationToken ct = default);
}
