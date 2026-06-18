using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Gamification.Models;
using PrintPlatform.Gamification.Tests.Helpers;
using Xunit;

namespace PrintPlatform.Gamification.Tests;

/// <summary>
/// Tests that leaderboard ranks are correctly computed and re-ordered as players accumulate XP.
/// </summary>
public sealed class LeaderboardRankTests
{
    private static async Task<long> SeedXpLeaderboardAsync(
        Microsoft.EntityFrameworkCore.DbContext db)
    {
        var board = new Leaderboard
        {
            Name   = "All-Time XP",
            Period = LeaderboardPeriod.AllTime,
            Type   = LeaderboardType.XP,
        };
        db.Set<Leaderboard>().Add(board);
        await db.SaveChangesAsync();
        return board.Id;
    }

    [Fact]
    public async Task Leaderboard_ThreePlayers_RankedByXPDescending()
    {
        // Arrange
        var (svc, db, _) = ServiceBuilder.Build();
        var boardId = await SeedXpLeaderboardAsync(db);

        await svc.EnsurePlayerAsync("rank-a", "Alice");
        await svc.EnsurePlayerAsync("rank-b", "Bob");
        await svc.EnsurePlayerAsync("rank-c", "Carol");

        // Award different XP amounts
        await svc.AwardPointsAsync("rank-a", GamificationActions.QcApproved, overridePoints: 300);  // 300
        await svc.AwardPointsAsync("rank-b", GamificationActions.QcApproved, overridePoints: 100);  // 100
        await svc.AwardPointsAsync("rank-c", GamificationActions.QcApproved, overridePoints: 200);  // 200

        // Act
        var entries = await svc.GetLeaderboardAsync(boardId, topN: 10);

        // Assert
        entries.Should().HaveCount(3);
        entries[0].Score.Should().Be(300); // Alice
        entries[0].Rank.Should().Be(1);
        entries[1].Score.Should().Be(200); // Carol
        entries[1].Rank.Should().Be(2);
        entries[2].Score.Should().Be(100); // Bob
        entries[2].Rank.Should().Be(3);
    }

    [Fact]
    public async Task Leaderboard_TiedScores_ShareSameRank()
    {
        var (svc, db, _) = ServiceBuilder.Build();
        var boardId = await SeedXpLeaderboardAsync(db);

        await svc.EnsurePlayerAsync("tie-a", "Tied1");
        await svc.EnsurePlayerAsync("tie-b", "Tied2");
        await svc.EnsurePlayerAsync("tie-c", "Better");

        await svc.AwardPointsAsync("tie-a", GamificationActions.QcApproved, overridePoints: 150);
        await svc.AwardPointsAsync("tie-b", GamificationActions.QcApproved, overridePoints: 150);
        await svc.AwardPointsAsync("tie-c", GamificationActions.QcApproved, overridePoints: 300);

        var entries = await svc.GetLeaderboardAsync(boardId, topN: 10);

        // Better player should be rank 1
        entries.First(e => e.Score == 300).Rank.Should().Be(1);

        // Both tied players share rank 2
        entries.Where(e => e.Score == 150).Should().OnlyContain(e => e.Rank == 2);
    }

    [Fact]
    public async Task Leaderboard_RankRecalculatesWhenPlayerEarnsMoreXP()
    {
        var (svc, db, _) = ServiceBuilder.Build();
        var boardId = await SeedXpLeaderboardAsync(db);

        await svc.EnsurePlayerAsync("catch-a", "Leader");
        await svc.EnsurePlayerAsync("catch-b", "Chaser");

        await svc.AwardPointsAsync("catch-a", GamificationActions.QcApproved, overridePoints: 500);
        await svc.AwardPointsAsync("catch-b", GamificationActions.QcApproved, overridePoints: 100);

        // Verify initial ranks
        var before = await svc.GetLeaderboardAsync(boardId, topN: 10);
        before.First(e => e.Score == 500).Rank.Should().Be(1);
        before.First(e => e.Score == 100).Rank.Should().Be(2);

        // Chaser overtakes by earning 500 more XP
        await svc.AwardPointsAsync("catch-b", GamificationActions.QcApproved, overridePoints: 500);

        var after = await svc.GetLeaderboardAsync(boardId, topN: 10);
        after.First(e => e.Score == 600).Rank.Should().Be(1, "chaser now has 600 and leads");
        after.First(e => e.Score == 500).Rank.Should().Be(2);
    }

    [Fact]
    public async Task Leaderboard_TopNRestriction_ExcludesLowerRanks()
    {
        var opts = new GamificationOptions { LeaderboardTopN = 2 };
        var (svc, db, _) = ServiceBuilder.Build(options: opts);
        var boardId = await SeedXpLeaderboardAsync(db);

        await svc.EnsurePlayerAsync("top-a", "A");
        await svc.EnsurePlayerAsync("top-b", "B");
        await svc.EnsurePlayerAsync("top-c", "C");

        await svc.AwardPointsAsync("top-a", GamificationActions.QcApproved, overridePoints: 300);
        await svc.AwardPointsAsync("top-b", GamificationActions.QcApproved, overridePoints: 200);
        await svc.AwardPointsAsync("top-c", GamificationActions.QcApproved, overridePoints: 100);

        var entries = await svc.GetLeaderboardAsync(boardId, topN: 2);

        entries.Should().HaveCount(2, "topN=2 limits to top 2 entries");
        entries.Should().NotContain(e => e.Score == 100);
    }

    [Fact]
    public async Task Leaderboard_SinglePlayer_IsAlwaysRank1()
    {
        var (svc, db, _) = ServiceBuilder.Build();
        var boardId = await SeedXpLeaderboardAsync(db);

        await svc.EnsurePlayerAsync("solo", "Solo");
        await svc.AwardPointsAsync("solo", GamificationActions.OrderDelivered);

        var entries = await svc.GetLeaderboardAsync(boardId);
        entries.Should().ContainSingle();
        entries[0].Rank.Should().Be(1);
    }
}
