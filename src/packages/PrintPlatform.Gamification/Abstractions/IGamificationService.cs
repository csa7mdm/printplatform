using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Abstractions;

/// <summary>
/// Primary entry-point for the host platform to interact with the gamification engine.
/// All operations are keyed on <paramref name="externalUserId"/> — no domain entity is required.
/// </summary>
public interface IGamificationService
{
    /// <summary>
    /// Ensures a gamification player record exists for the given external user.
    /// Idempotent — safe to call on every login or first-action.
    /// </summary>
    /// <param name="externalUserId">Opaque identifier from the host platform.</param>
    /// <param name="displayName">Display name shown on leaderboards.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GamificationPlayer> EnsurePlayerAsync(
        string externalUserId,
        string displayName,
        CancellationToken ct = default);

    /// <summary>
    /// Awards XP for the given action, then triggers the full side-effect pipeline:
    /// level-up detection, achievement evaluation, streak update, and leaderboard refresh.
    /// Raises one or more <c>INotification</c> events via MediatR.
    /// </summary>
    /// <param name="externalUserId">Player to award points to.</param>
    /// <param name="action">
    /// Action string — use <see cref="GamificationActions"/> constants or a custom host value.
    /// The XP amount is resolved from <see cref="GamificationOptions"/> unless
    /// <paramref name="overridePoints"/> is supplied.
    /// </param>
    /// <param name="overridePoints">
    /// When non-null, bypasses the options-based XP map and uses this value directly.
    /// Useful for challenge completion rewards.
    /// </param>
    /// <param name="metadataJson">
    /// Optional JSON context persisted in the <see cref="PointTransaction"/> (never parsed here).
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task AwardPointsAsync(
        string externalUserId,
        string action,
        int? overridePoints = null,
        string? metadataJson = null,
        CancellationToken ct = default);

    /// <summary>Returns the current snapshot for a player, or <c>null</c> if not found.</summary>
    Task<GamificationPlayer?> GetPlayerAsync(string externalUserId, CancellationToken ct = default);

    /// <summary>Returns all achievements unlocked by the player, ordered by unlock date.</summary>
    Task<IReadOnlyList<PlayerAchievement>> GetPlayerAchievementsAsync(
        string externalUserId,
        CancellationToken ct = default);

    /// <summary>Returns active streaks for the player.</summary>
    Task<IReadOnlyList<Streak>> GetStreaksAsync(string externalUserId, CancellationToken ct = default);

    /// <summary>Returns the top-N entries for a leaderboard, ordered by rank ascending.</summary>
    Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardAsync(
        long leaderboardId,
        int topN = 50,
        CancellationToken ct = default);

    /// <summary>
    /// Records incremental progress on a challenge and awards XP if completion is reached.
    /// </summary>
    Task RecordChallengeProgressAsync(
        string externalUserId,
        long challengeId,
        int increment = 1,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the XP score for all XP-type leaderboard entries for a player.
    /// Called internally by <see cref="AwardPointsAsync"/>; exposed for manual refresh scenarios.
    /// </summary>
    Task RefreshLeaderboardScoresAsync(string externalUserId, CancellationToken ct = default);
}
