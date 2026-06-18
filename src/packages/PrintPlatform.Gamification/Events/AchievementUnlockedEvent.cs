using MediatR;

namespace PrintPlatform.Gamification.Events;

/// <summary>
/// Published when a player satisfies the criteria for an achievement for the first time.
/// </summary>
/// <param name="ExternalUserId">Opaque host-platform user identifier.</param>
/// <param name="AchievementId">PK of the unlocked <c>Achievement</c> row.</param>
/// <param name="AchievementName">Human-readable name (copied for convenience).</param>
/// <param name="IconSlug">Icon slug for immediate UI rendering without a second query.</param>
/// <param name="BonusXP">Additional XP granted alongside the achievement unlock.</param>
/// <param name="UnlockedAt">UTC timestamp of the unlock.</param>
public sealed record AchievementUnlockedEvent(
    string ExternalUserId,
    long AchievementId,
    string AchievementName,
    string IconSlug,
    int BonusXP,
    DateTimeOffset UnlockedAt) : INotification;
