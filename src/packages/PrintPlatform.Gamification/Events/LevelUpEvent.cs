using MediatR;

namespace PrintPlatform.Gamification.Events;

/// <summary>
/// Published when a player's XP crosses a level threshold, moving them to a higher tier.
/// </summary>
/// <param name="ExternalUserId">Opaque host-platform user identifier.</param>
/// <param name="OldLevel">Tier the player was on before this award.</param>
/// <param name="NewLevel">Tier the player has just reached.</param>
/// <param name="NewLevelName">Human-readable name of the new tier (e.g. "Expert").</param>
/// <param name="TotalXP">Player's total XP at the moment of level-up.</param>
public sealed record LevelUpEvent(
    string ExternalUserId,
    int OldLevel,
    int NewLevel,
    string NewLevelName,
    int TotalXP) : INotification;
