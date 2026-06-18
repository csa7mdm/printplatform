using Microsoft.EntityFrameworkCore;
using PrintPlatform.Gamification.Abstractions;
using PrintPlatform.Gamification.Data;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Rules;

/// <summary>
/// Awards milestone badges at 1 / 10 / 50 / 100 / 500 completed jobs.
/// Each milestone maps to a distinct <see cref="Achievement"/> row in <c>gam_achievements</c>.
/// Triggered only by <c>OrderDelivered</c> or <c>FirstJobCompleted</c> actions.
/// </summary>
public sealed class MilestoneJobRule : IAchievementRule
{
    // Stable achievement IDs for seeded milestone rows.
    public const long Job1Id   = 10L;
    public const long Job10Id  = 11L;
    public const long Job50Id  = 12L;
    public const long Job100Id = 13L;
    public const long Job500Id = 14L;

    private static readonly IReadOnlyDictionary<long, int> MilestoneMap = new Dictionary<long, int>
    {
        [Job1Id]   = 1,
        [Job10Id]  = 10,
        [Job50Id]  = 50,
        [Job100Id] = 100,
        [Job500Id] = 500,
    };

    private readonly long _achievementId;
    private readonly int  _targetCount;
    private readonly GamificationDbContext _db;

    public MilestoneJobRule(long achievementId, GamificationDbContext db)
    {
        if (!MilestoneMap.TryGetValue(achievementId, out _targetCount))
            throw new ArgumentOutOfRangeException(nameof(achievementId),
                $"Unknown milestone achievement id {achievementId}.");
        _achievementId = achievementId;
        _db = db;
    }

    /// <inheritdoc />
    public long AchievementId => _achievementId;

    /// <inheritdoc />
    public async Task<bool> IsSatisfiedAsync(
        GamificationPlayer player,
        AchievementContext context,
        CancellationToken ct = default)
    {
        if (context.Action is not (GamificationActions.OrderDelivered
                               or GamificationActions.FirstJobCompleted))
            return false;

        // Count all delivered / completed transactions to get total jobs.
        var completedJobs = await _db.Transactions
            .AsNoTracking()
            .CountAsync(
                t => t.PlayerId == player.Id
                  && (t.Action == GamificationActions.OrderDelivered
                   || t.Action == GamificationActions.FirstJobCompleted),
                ct);

        return completedJobs >= _targetCount;
    }

    // ── Factory helpers ───────────────────────────────────────────────────────

    /// <summary>Creates one <see cref="MilestoneJobRule"/> for every milestone achievement ID.</summary>
    public static IEnumerable<IAchievementRule> CreateAll(GamificationDbContext db) =>
    [
        new MilestoneJobRule(Job1Id,   db),
        new MilestoneJobRule(Job10Id,  db),
        new MilestoneJobRule(Job50Id,  db),
        new MilestoneJobRule(Job100Id, db),
        new MilestoneJobRule(Job500Id, db),
    ];
}
