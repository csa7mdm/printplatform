using MediatR;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Gamification.Abstractions;
using PrintPlatform.Gamification.Data;
using PrintPlatform.Gamification.Events;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Services;

/// <summary>
/// Evaluates all registered <see cref="IAchievementRule"/> implementations after each
/// XP award and persists newly unlocked <see cref="PlayerAchievement"/> records.
/// Each rule is only evaluated when the player has not yet unlocked the corresponding achievement.
/// </summary>
public sealed class AchievementEngine
{
    private readonly GamificationDbContext _db;
    private readonly IEnumerable<IAchievementRule> _rules;
    private readonly IMediator _mediator;

    public AchievementEngine(
        GamificationDbContext db,
        IEnumerable<IAchievementRule> rules,
        IMediator mediator)
    {
        _db = db;
        _rules = rules;
        _mediator = mediator;
    }

    /// <summary>
    /// Runs all registered rules against <paramref name="player"/> for the given
    /// <paramref name="context"/>.  For each newly satisfied rule:
    /// <list type="number">
    ///   <item>Inserts a <see cref="PlayerAchievement"/> row.</item>
    ///   <item>Publishes an <see cref="AchievementUnlockedEvent"/>.</item>
    /// </list>
    /// Returns the list of newly unlocked achievement IDs.
    /// </summary>
    public async Task<IReadOnlyList<long>> EvaluateAsync(
        GamificationPlayer player,
        AchievementContext context,
        CancellationToken ct = default)
    {
        // Fetch IDs of achievements already unlocked — avoids re-awarding.
        var alreadyUnlocked = await _db.PlayerAchievements
            .AsNoTracking()
            .Where(pa => pa.PlayerId == player.Id)
            .Select(pa => pa.AchievementId)
            .ToHashSetAsync(ct);

        // Cache achievement metadata to avoid N+1 queries.
        var achievementIds = _rules.Select(r => r.AchievementId).Distinct().ToArray();
        var achievements = await _db.Achievements
            .AsNoTracking()
            .Where(a => achievementIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, ct);

        var newlyUnlocked = new List<long>();
        var now = DateTimeOffset.UtcNow;

        foreach (var rule in _rules)
        {
            if (alreadyUnlocked.Contains(rule.AchievementId))
                continue;

            if (!achievements.TryGetValue(rule.AchievementId, out var achievement))
                continue;   // achievement not seeded in DB — skip

            bool satisfied = await rule.IsSatisfiedAsync(player, context, ct);
            if (!satisfied) continue;

            // Mark as unlocked.
            var playerAchievement = new PlayerAchievement
            {
                PlayerId      = player.Id,
                AchievementId = achievement.Id,
                UnlockedAt    = now,
            };
            _db.PlayerAchievements.Add(playerAchievement);
            newlyUnlocked.Add(achievement.Id);

            await _mediator.Publish(
                new AchievementUnlockedEvent(
                    player.ExternalUserId,
                    achievement.Id,
                    achievement.Name,
                    achievement.IconSlug,
                    achievement.BonusXP,
                    now),
                ct);
        }

        if (newlyUnlocked.Count > 0)
            await _db.SaveChangesAsync(ct);

        return newlyUnlocked;
    }
}
