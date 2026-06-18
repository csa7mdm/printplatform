using Microsoft.EntityFrameworkCore;
using PrintPlatform.Gamification.Abstractions;
using PrintPlatform.Gamification.Data;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Rules;

/// <summary>
/// "Perfectionist" badge — awarded when a player receives 10 consecutive QC approvals
/// without a single QC rejection in between.
/// The streak counter is stored in the <c>gam_streaks</c> table (ActionType = "QCApproved").
/// A <c>QCRejected</c> action resets the counter to 0 before any evaluation.
/// </summary>
public sealed class ConsecutiveQCApprovalRule : IAchievementRule
{
    /// <summary>
    /// Stable ID for the "Perfectionist" achievement row seeded into <c>gam_achievements</c>.
    /// If you change the seed data ID, update this constant.
    /// </summary>
    public const long PerfectionistAchievementId = 1L;

    private const int RequiredConsecutiveApprovals = 10;

    /// <inheritdoc />
    public long AchievementId => PerfectionistAchievementId;

    private readonly GamificationDbContext _db;

    public ConsecutiveQCApprovalRule(GamificationDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<bool> IsSatisfiedAsync(
        GamificationPlayer player,
        AchievementContext context,
        CancellationToken ct = default)
    {
        // Only relevant when a QC approval just occurred.
        if (context.Action != GamificationActions.QcApproved)
            return false;

        var streak = await _db.Streaks
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.PlayerId == player.Id && s.ActionType == GamificationActions.QcApproved,
                ct);

        // CurrentCount was already incremented by StreakService before rule evaluation.
        return streak is not null && streak.CurrentCount >= RequiredConsecutiveApprovals;
    }
}
