using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Abstractions;

/// <summary>
/// Pluggable rule that determines whether a player has satisfied an achievement's criteria.
/// Implement and register via DI to add custom achievement logic without modifying the engine core.
/// </summary>
/// <remarks>
/// Rules are evaluated by <c>AchievementEngine</c> after every <c>AwardPointsAsync</c> call.
/// A rule should be stateless and fast — it receives a pre-loaded context snapshot.
/// </remarks>
public interface IAchievementRule
{
    /// <summary>
    /// The unique <see cref="Achievement.Id"/> this rule evaluates.
    /// The engine calls this rule only for the matching achievement row.
    /// </summary>
    long AchievementId { get; }

    /// <summary>
    /// Evaluates whether <paramref name="player"/> has met the achievement criteria
    /// given the triggering <paramref name="context"/>.
    /// </summary>
    /// <param name="player">Current player snapshot (includes navigation collections).</param>
    /// <param name="context">Details of the action that just occurred.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if the achievement should be unlocked.</returns>
    Task<bool> IsSatisfiedAsync(
        GamificationPlayer player,
        AchievementContext context,
        CancellationToken ct = default);
}

/// <summary>
/// Contextual data provided to <see cref="IAchievementRule.IsSatisfiedAsync"/>.
/// </summary>
/// <param name="Action">The action string that triggered the evaluation pass.</param>
/// <param name="PointsAwarded">XP amount awarded in this transaction.</param>
/// <param name="MetadataJson">Optional JSON metadata supplied by the host.</param>
/// <param name="OccurredAt">UTC timestamp of the triggering event.</param>
public sealed record AchievementContext(
    string Action,
    int PointsAwarded,
    string? MetadataJson,
    DateTimeOffset OccurredAt);
