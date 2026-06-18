using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PrintPlatform.Gamification.Abstractions;
using PrintPlatform.Gamification.Data;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Services;

/// <summary>
/// Upserts <see cref="LeaderboardEntry"/> rows and recomputes ranks after each score change.
/// Built-in <see cref="ILeaderboardProvider"/> for <see cref="LeaderboardType.XP"/> is inlined;
/// other types are delegated to registered <see cref="ILeaderboardProvider"/> implementations.
/// </summary>
public sealed class LeaderboardService
{
    private readonly GamificationDbContext _db;
    private readonly IEnumerable<ILeaderboardProvider> _providers;
    private readonly GamificationOptions _options;

    public LeaderboardService(
        GamificationDbContext db,
        IEnumerable<ILeaderboardProvider> providers,
        IOptions<GamificationOptions> options)
    {
        _db = db;
        _providers = providers;
        _options = options.Value;
    }

    /// <summary>
    /// Updates all leaderboard entries for the player and re-ranks the affected leaderboards.
    /// </summary>
    public async Task RefreshPlayerScoresAsync(
        GamificationPlayer player,
        CancellationToken ct = default)
    {
        var leaderboards = await _db.Leaderboards.AsNoTracking().ToListAsync(ct);
        if (leaderboards.Count == 0) return;

        var now = DateTimeOffset.UtcNow;

        foreach (var board in leaderboards)
        {
            var score = await ComputeScoreAsync(player, board, ct);

            var entry = await _db.LeaderboardEntries
                .FirstOrDefaultAsync(e => e.LeaderboardId == board.Id && e.PlayerId == player.Id, ct);

            if (entry is null)
            {
                entry = new LeaderboardEntry
                {
                    LeaderboardId = board.Id,
                    PlayerId      = player.Id,
                    Score         = score,
                    Rank          = 0,          // assigned below
                    UpdatedAt     = now,
                };
                _db.LeaderboardEntries.Add(entry);
            }
            else
            {
                entry.Score     = score;
                entry.UpdatedAt = now;
            }
        }

        await _db.SaveChangesAsync(ct);

        // Re-rank each affected leaderboard.
        var affectedIds = leaderboards.Select(l => l.Id).ToHashSet();
        await RecomputeRanksAsync(affectedIds, ct);
    }

    /// <summary>Returns the top-N entries for a leaderboard ordered by rank.</summary>
    public async Task<IReadOnlyList<LeaderboardEntry>> GetTopAsync(
        long leaderboardId,
        int topN,
        CancellationToken ct = default)
    {
        return await _db.LeaderboardEntries
            .AsNoTracking()
            .Where(e => e.LeaderboardId == leaderboardId && e.Rank > 0 && e.Rank <= topN)
            .OrderBy(e => e.Rank)
            .ToListAsync(ct);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<double> ComputeScoreAsync(
        GamificationPlayer player,
        Leaderboard board,
        CancellationToken ct)
    {
        if (board.Type == LeaderboardType.XP)
            return player.TotalXP;

        var provider = _providers.FirstOrDefault(p => p.SupportedType == board.Type);
        if (provider is null)
            return 0;

        return await provider.ComputeScoreAsync(player, board, ct);
    }

    /// <summary>
    /// Dense-rank all entries per leaderboard (ties share the same rank).
    /// Only processes top-N entries per <see cref="GamificationOptions.LeaderboardTopN"/>.
    /// </summary>
    private async Task RecomputeRanksAsync(HashSet<long> leaderboardIds, CancellationToken ct)
    {
        foreach (var boardId in leaderboardIds)
        {
            // Load all entries for the board, ordered by score descending.
            var entries = await _db.LeaderboardEntries
                .Where(e => e.LeaderboardId == boardId)
                .OrderByDescending(e => e.Score)
                .ToListAsync(ct);

            int rank = 0;
            double? lastScore = null;
            int displayRank = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                rank++;
                if (entries[i].Score != lastScore)
                {
                    displayRank = rank;
                    lastScore   = entries[i].Score;
                }
                // Only store rank for entries within the top-N window.
                entries[i].Rank = displayRank <= _options.LeaderboardTopN ? displayRank : 0;
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
