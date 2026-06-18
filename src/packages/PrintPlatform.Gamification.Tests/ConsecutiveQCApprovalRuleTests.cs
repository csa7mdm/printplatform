using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Gamification.Abstractions;
using PrintPlatform.Gamification.Events;
using PrintPlatform.Gamification.Models;
using PrintPlatform.Gamification.Rules;
using PrintPlatform.Gamification.Tests.Helpers;
using Xunit;

namespace PrintPlatform.Gamification.Tests;

/// <summary>
/// Tests for <see cref="ConsecutiveQCApprovalRule"/>:
/// fires the "Perfectionist" achievement at exactly 10 consecutive QC approvals
/// and resets to zero on a QC rejection.
/// </summary>
public sealed class ConsecutiveQCApprovalRuleTests
{
    private static Achievement PerfectionistAchievement() => new()
    {
        Id           = ConsecutiveQCApprovalRule.PerfectionistAchievementId,
        Name         = "Perfectionist",
        Description  = "10 consecutive QC approvals",
        IconSlug     = "badge-perfectionist",
        Category     = "Quality",
        CriteriaJson = """{"count":10,"action":"QCApproved"}""",
        BonusXP      = 50,
    };

    [Fact]
    public async Task ConsecutiveQCRule_AtCount10_UnlocksAchievement()
    {
        // Arrange — build db first so it can be passed to the rule constructor.
        var db       = GamificationDbContextFactory.Create();
        var mediator = new FakeMediator();
        var (svc, _, _) = ServiceBuilder.BuildWithDb(db, mediator,
            rules: [new ConsecutiveQCApprovalRule(db)]);

        db.Achievements.Add(PerfectionistAchievement());
        await db.SaveChangesAsync();

        await svc.EnsurePlayerAsync("qc-user", "QC Master");

        // Act — award 10 QC approvals
        for (int i = 0; i < 10; i++)
            await svc.AwardPointsAsync("qc-user", GamificationActions.QcApproved);

        // Assert
        var unlockEvent = mediator.Published.OfType<AchievementUnlockedEvent>()
            .FirstOrDefault(e => e.AchievementId == ConsecutiveQCApprovalRule.PerfectionistAchievementId);

        unlockEvent.Should().NotBeNull("achievement should fire at streak count = 10");
        unlockEvent!.AchievementName.Should().Be("Perfectionist");
        unlockEvent.ExternalUserId.Should().Be("qc-user");
    }

    [Fact]
    public async Task ConsecutiveQCRule_AtCount9_DoesNotUnlockAchievement()
    {
        var db       = GamificationDbContextFactory.Create();
        var mediator = new FakeMediator();
        var (svc, _, _) = ServiceBuilder.BuildWithDb(db, mediator,
            rules: [new ConsecutiveQCApprovalRule(db)]);

        db.Achievements.Add(PerfectionistAchievement());
        await db.SaveChangesAsync();

        await svc.EnsurePlayerAsync("qc-user-b", "Almost");

        for (int i = 0; i < 9; i++)
            await svc.AwardPointsAsync("qc-user-b", GamificationActions.QcApproved);

        mediator.Published.OfType<AchievementUnlockedEvent>()
            .Should().NotContain(e => e.AchievementId == ConsecutiveQCApprovalRule.PerfectionistAchievementId);
    }

    [Fact]
    public async Task ConsecutiveQCRule_RejectionAfter5Approvals_ResetsStreak()
    {
        var db       = GamificationDbContextFactory.Create();
        var mediator = new FakeMediator();
        var (svc, _, _) = ServiceBuilder.BuildWithDb(db, mediator,
            rules: [new ConsecutiveQCApprovalRule(db)]);

        db.Achievements.Add(PerfectionistAchievement());
        await db.SaveChangesAsync();

        await svc.EnsurePlayerAsync("qc-user-c", "Reset User");

        // 5 approvals
        for (int i = 0; i < 5; i++)
            await svc.AwardPointsAsync("qc-user-c", GamificationActions.QcApproved);

        // Rejection — should reset streak to 0
        await svc.AwardPointsAsync("qc-user-c", GamificationActions.QcRejected);

        var player = await db.Players
            .FirstAsync(p => p.ExternalUserId == "qc-user-c");

        var streak = await db.Streaks
            .FirstOrDefaultAsync(s => s.PlayerId == player.Id
                && s.ActionType == GamificationActions.QcApproved);

        streak.Should().NotBeNull();
        streak!.CurrentCount.Should().Be(0, "rejection resets the QCApproved streak");
    }

    [Fact]
    public async Task ConsecutiveQCRule_RejectionThenRestartApprovals_DoesNotFireUntil10Again()
    {
        var db       = GamificationDbContextFactory.Create();
        var mediator = new FakeMediator();
        var (svc, _, _) = ServiceBuilder.BuildWithDb(db, mediator,
            rules: [new ConsecutiveQCApprovalRule(db)]);

        db.Achievements.Add(PerfectionistAchievement());
        await db.SaveChangesAsync();

        await svc.EnsurePlayerAsync("qc-user-d", "Restart");

        // 7 approvals, then a rejection
        for (int i = 0; i < 7; i++)
            await svc.AwardPointsAsync("qc-user-d", GamificationActions.QcApproved);

        await svc.AwardPointsAsync("qc-user-d", GamificationActions.QcRejected);

        // Only 9 more approvals after the reset — should NOT fire
        for (int i = 0; i < 9; i++)
            await svc.AwardPointsAsync("qc-user-d", GamificationActions.QcApproved);

        mediator.Published.OfType<AchievementUnlockedEvent>()
            .Should().NotContain(e => e.AchievementId == ConsecutiveQCApprovalRule.PerfectionistAchievementId,
                "9 approvals after reset is one short of threshold");
    }

    [Fact]
    public async Task ConsecutiveQCRule_OnlyFiresOnce_EvenAfterMoreApprovals()
    {
        var db       = GamificationDbContextFactory.Create();
        var mediator = new FakeMediator();
        var (svc, _, _) = ServiceBuilder.BuildWithDb(db, mediator,
            rules: [new ConsecutiveQCApprovalRule(db)]);

        db.Achievements.Add(PerfectionistAchievement());
        await db.SaveChangesAsync();

        await svc.EnsurePlayerAsync("qc-user-e", "Double Check");

        // 15 approvals
        for (int i = 0; i < 15; i++)
            await svc.AwardPointsAsync("qc-user-e", GamificationActions.QcApproved);

        var unlockEvents = mediator.Published.OfType<AchievementUnlockedEvent>()
            .Where(e => e.AchievementId == ConsecutiveQCApprovalRule.PerfectionistAchievementId)
            .ToList();

        unlockEvents.Should().HaveCount(1, "achievement is unlocked exactly once");
    }
}
