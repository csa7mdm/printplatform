using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PrintPlatform.Gamification.Abstractions;
using PrintPlatform.Gamification.Data;
using PrintPlatform.Gamification.Events;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Services;

/// <summary>
/// Full implementation of <see cref="IGamificationService"/>.
/// Orchestrates the five-step pipeline on every <see cref="AwardPointsAsync"/> call:
/// <list type="number">
///   <item>Append a <see cref="PointTransaction"/>.</item>
///   <item>Recalculate <see cref="GamificationPlayer.TotalXP"/>.</item>
///   <item>Detect level-up and raise <see cref="LevelUpEvent"/>.</item>
///   <item>Run <see cref="AchievementEngine"/> and raise per-achievement events.</item>
///   <item>Update streaks via <see cref="StreakService"/>.</item>
///   <item>Refresh leaderboard scores via <see cref="LeaderboardService"/>.</item>
/// </list>
/// </summary>
public sealed class GamificationService : IGamificationService
{
    private readonly GamificationDbContext _db;
    private readonly IMediator            _mediator;
    private readonly GamificationOptions  _options;
    private readonly AchievementEngine    _achievementEngine;
    private readonly StreakService        _streakService;
    private readonly LeaderboardService   _leaderboardService;

    public GamificationService(
        GamificationDbContext db,
        IMediator mediator,
        IOptions<GamificationOptions> options,
        AchievementEngine achievementEngine,
        StreakService streakService,
        LeaderboardService leaderboardService)
    {
        _db                 = db;
        _mediator           = mediator;
        _options            = options.Value;
        _achievementEngine  = achievementEngine;
        _streakService      = streakService;
        _leaderboardService = leaderboardService;
    }

    // ── EnsurePlayer ──────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<GamificationPlayer> EnsurePlayerAsync(
        string externalUserId,
        string displayName,
        CancellationToken ct = default)
    {
        var player = await _db.Players
            .FirstOrDefaultAsync(p => p.ExternalUserId == externalUserId, ct);

        if (player is null)
        {
            player = new GamificationPlayer
            {
                ExternalUserId = externalUserId,
                DisplayName    = displayName,
                TotalXP        = 0,
                CurrentLevel   = 1,
                CreatedAt      = DateTimeOffset.UtcNow,
            };
            _db.Players.Add(player);
            await _db.SaveChangesAsync(ct);
        }
        else if (player.DisplayName != displayName)
        {
            player.DisplayName = displayName;
            await _db.SaveChangesAsync(ct);
        }

        return player;
    }

    // ── AwardPoints (main pipeline) ───────────────────────────────────────────

    /// <inheritdoc />
    public async Task AwardPointsAsync(
        string externalUserId,
        string action,
        int? overridePoints = null,
        string? metadataJson = null,
        CancellationToken ct = default)
    {
        var player = await RequirePlayerAsync(externalUserId, ct);
        var now    = DateTimeOffset.UtcNow;

        int xp = overridePoints ?? _options.ResolveXp(action);

        // ── Step 1: Append PointTransaction ───────────────────────────────────
        if (xp > 0)
        {
            var tx = new PointTransaction
            {
                PlayerId     = player.Id,
                Points       = xp,
                Action       = action,
                MetadataJson = metadataJson,
                CreatedAt    = now,
            };
            _db.Transactions.Add(tx);

            // ── Step 2: Recalculate TotalXP ───────────────────────────────────
            int previousLevel = player.CurrentLevel;
            player.TotalXP       += xp;
            player.LastActivityAt = now;

            var newLevelDef = _options.ResolveLevel(player.TotalXP);
            player.CurrentLevel = newLevelDef.Tier;

            await _db.SaveChangesAsync(ct);

            // ── Step 3: Level-up event ─────────────────────────────────────────
            if (newLevelDef.Tier > previousLevel)
            {
                await _mediator.Publish(
                    new LevelUpEvent(
                        externalUserId,
                        previousLevel,
                        newLevelDef.Tier,
                        newLevelDef.Name,
                        player.TotalXP),
                    ct);
            }

            // Publish points-awarded event.
            await _mediator.Publish(
                new PointsAwardedEvent(externalUserId, xp, action, player.TotalXP, metadataJson),
                ct);
        }

        // ── Step 4: Streak update (must run BEFORE achievement engine so
        //           streak-based rules see the already-incremented counter) ────
        await _streakService.ProcessAsync(player, action, now, ct);

        // ── Step 5: Achievement engine ─────────────────────────────────────────
        // Reload player with navigation so rules can inspect collections.
        var playerWithNav = await _db.Players
            .Include(p => p.Achievements)
            .Include(p => p.Transactions)
            .FirstAsync(p => p.Id == player.Id, ct);

        var context = new AchievementContext(action, xp, metadataJson, now);
        await _achievementEngine.EvaluateAsync(playerWithNav, context, ct);

        // ── Step 6: Leaderboard refresh ───────────────────────────────────────
        if (xp > 0)
            await _leaderboardService.RefreshPlayerScoresAsync(player, ct);
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<GamificationPlayer?> GetPlayerAsync(
        string externalUserId,
        CancellationToken ct = default) =>
        await _db.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ExternalUserId == externalUserId, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlayerAchievement>> GetPlayerAchievementsAsync(
        string externalUserId,
        CancellationToken ct = default)
    {
        var player = await RequirePlayerAsync(externalUserId, ct);
        return await _db.PlayerAchievements
            .AsNoTracking()
            .Include(pa => pa.Achievement)
            .Where(pa => pa.PlayerId == player.Id)
            .OrderBy(pa => pa.UnlockedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Streak>> GetStreaksAsync(
        string externalUserId,
        CancellationToken ct = default)
    {
        var player = await RequirePlayerAsync(externalUserId, ct);
        return await _db.Streaks
            .AsNoTracking()
            .Where(s => s.PlayerId == player.Id && s.CurrentCount > 0)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardAsync(
        long leaderboardId,
        int topN = 50,
        CancellationToken ct = default) =>
        _leaderboardService.GetTopAsync(leaderboardId, topN, ct);

    /// <inheritdoc />
    public async Task RecordChallengeProgressAsync(
        string externalUserId,
        long challengeId,
        int increment = 1,
        CancellationToken ct = default)
    {
        var player = await RequirePlayerAsync(externalUserId, ct);

        var challenge = await _db.Challenges
            .FirstOrDefaultAsync(c => c.Id == challengeId && c.IsActive, ct)
            ?? throw new InvalidOperationException($"Active challenge {challengeId} not found.");

        var now = DateTimeOffset.UtcNow;
        if (now < challenge.StartDate || now > challenge.EndDate)
            return; // outside challenge window — no-op

        var entry = await _db.PlayerChallenges
            .FirstOrDefaultAsync(pc => pc.PlayerId == player.Id && pc.ChallengeId == challengeId, ct);

        if (entry is null)
        {
            entry = new PlayerChallenge
            {
                PlayerId    = player.Id,
                ChallengeId = challengeId,
                Progress    = 0,
            };
            _db.PlayerChallenges.Add(entry);
        }

        if (entry.CompletedAt.HasValue)
            return; // already completed

        entry.Progress += increment;

        // Check completion by parsing CriteriaJson's targetCount field.
        int target = ParseTargetCount(challenge.CriteriaJson);
        if (target > 0 && entry.Progress >= target)
        {
            entry.CompletedAt = now;
            await _db.SaveChangesAsync(ct);

            // Award challenge XP.
            await AwardPointsAsync(
                externalUserId,
                GamificationActions.ChallengeCompleted,
                overridePoints: challenge.RewardXP,
                metadataJson: $"{{\"challengeId\": {challengeId}}}",
                ct);
        }
        else
        {
            await _db.SaveChangesAsync(ct);
        }
    }

    /// <inheritdoc />
    public async Task RefreshLeaderboardScoresAsync(
        string externalUserId,
        CancellationToken ct = default)
    {
        var player = await RequirePlayerAsync(externalUserId, ct);
        await _leaderboardService.RefreshPlayerScoresAsync(player, ct);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<GamificationPlayer> RequirePlayerAsync(
        string externalUserId,
        CancellationToken ct)
    {
        var player = await _db.Players
            .FirstOrDefaultAsync(p => p.ExternalUserId == externalUserId, ct);

        return player
            ?? throw new InvalidOperationException(
                $"Gamification player '{externalUserId}' not found. Call EnsurePlayerAsync first.");
    }

    private static int ParseTargetCount(string criteriaJson)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(criteriaJson);
            if (doc.RootElement.TryGetProperty("targetCount", out var el)
                && el.TryGetInt32(out int val))
                return val;
        }
        catch { /* malformed JSON — treat as no target */ }
        return 0;
    }
}
