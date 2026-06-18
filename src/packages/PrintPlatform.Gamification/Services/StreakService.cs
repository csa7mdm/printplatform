using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PrintPlatform.Gamification.Data;
using PrintPlatform.Gamification.Events;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Services;

/// <summary>
/// Maintains per-player, per-action streak counters.
/// A streak increments on each qualifying action and resets when either:
/// <list type="bullet">
///   <item>The gap since the last action exceeds <see cref="GamificationOptions.StreakBreakThresholdHours"/>.</item>
///   <item>An explicit reset action is received (e.g. <c>QCRejected</c> resets the <c>QCApproved</c> streak).</item>
/// </list>
/// </summary>
public sealed class StreakService
{
    private readonly GamificationDbContext _db;
    private readonly IMediator _mediator;
    private readonly GamificationOptions _options;

    /// <summary>
    /// Maps an action that should reset a related streak to the streak's action type.
    /// E.g. <c>QCRejected</c> → resets the <c>QCApproved</c> streak.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> ResetMap = new Dictionary<string, string>
    {
        [GamificationActions.QcRejected] = GamificationActions.QcApproved,
    };

    public StreakService(
        GamificationDbContext db,
        IMediator mediator,
        IOptions<GamificationOptions> options)
    {
        _db = db;
        _mediator = mediator;
        _options = options.Value;
    }

    /// <summary>
    /// Processes the action for streak tracking.
    /// Returns the updated (or newly created) streak, or null for actions that don't track streaks.
    /// </summary>
    public async Task<Streak?> ProcessAsync(
        GamificationPlayer player,
        string action,
        DateTimeOffset occurredAt,
        CancellationToken ct = default)
    {
        // Handle reset actions first.
        if (ResetMap.TryGetValue(action, out var streakToReset))
        {
            await ResetStreakAsync(player.Id, streakToReset, ct);
            return null;
        }

        // Only track streaks for specific actions.
        if (!IsTrackedAction(action))
            return null;

        var streak = await _db.Streaks
            .FirstOrDefaultAsync(s => s.PlayerId == player.Id && s.ActionType == action, ct);

        if (streak is null)
        {
            streak = new Streak
            {
                PlayerId       = player.Id,
                ActionType     = action,
                CurrentCount   = 1,
                BestCount      = 1,
                LastActionDate = occurredAt,
            };
            _db.Streaks.Add(streak);
        }
        else
        {
            var gapHours = (occurredAt - streak.LastActionDate).TotalHours;

            if (gapHours > _options.StreakBreakThresholdHours)
            {
                // Streak broken — restart from 1.
                streak.CurrentCount = 1;
            }
            else
            {
                streak.CurrentCount++;
            }

            streak.LastActionDate = occurredAt;
            if (streak.CurrentCount > streak.BestCount)
                streak.BestCount = streak.CurrentCount;
        }

        await _db.SaveChangesAsync(ct);

        // Publish milestone events if applicable.
        if (Array.IndexOf(_options.StreakMilestones, streak.CurrentCount) >= 0)
        {
            await _mediator.Publish(
                new StreakMilestoneEvent(player.ExternalUserId, action, streak.CurrentCount),
                ct);
        }

        return streak;
    }

    private async Task ResetStreakAsync(long playerId, string actionType, CancellationToken ct)
    {
        var streak = await _db.Streaks
            .FirstOrDefaultAsync(s => s.PlayerId == playerId && s.ActionType == actionType, ct);

        if (streak is null || streak.CurrentCount == 0)
            return;

        streak.CurrentCount = 0;
        await _db.SaveChangesAsync(ct);
    }

    private static bool IsTrackedAction(string action) => action is
        GamificationActions.QcApproved or
        GamificationActions.OrderDelivered or
        GamificationActions.JobAccepted;
}
