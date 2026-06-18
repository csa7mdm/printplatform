using MediatR;

namespace PrintPlatform.Gamification.Events;

/// <summary>
/// Published when a player's streak count hits one of the configured milestone values
/// (see <see cref="GamificationOptions.StreakMilestones"/>).
/// </summary>
/// <param name="ExternalUserId">Opaque host-platform user identifier.</param>
/// <param name="ActionType">The action whose streak reached the milestone.</param>
/// <param name="Count">Streak count at the milestone (e.g. 7 for a 7-day streak).</param>
public sealed record StreakMilestoneEvent(
    string ExternalUserId,
    string ActionType,
    int Count) : INotification;
