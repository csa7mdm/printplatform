using PrintPlatform.Gamification.Abstractions;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Rules;

/// <summary>
/// "Complete Profile" achievement — awarded the first time a player triggers
/// the <c>ProfileCompleted</c> action, confirming all required profile fields
/// have been filled on the host platform.
/// </summary>
public sealed class ProfileCompleteRule : IAchievementRule
{
    public const long ProfileCompleteAchievementId = 30L;

    /// <inheritdoc />
    public long AchievementId => ProfileCompleteAchievementId;

    /// <inheritdoc />
    public Task<bool> IsSatisfiedAsync(
        GamificationPlayer player,
        AchievementContext context,
        CancellationToken ct = default)
    {
        // The achievement engine only calls this rule when the achievement is not
        // yet unlocked, so a simple action-string check is sufficient.
        var satisfied = context.Action == GamificationActions.ProfileCompleted;
        return Task.FromResult(satisfied);
    }
}
