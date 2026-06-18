using FluentAssertions;
using PrintPlatform.Gamification.Events;
using PrintPlatform.Gamification.Tests.Helpers;
using Xunit;

namespace PrintPlatform.Gamification.Tests;

/// <summary>
/// Tests that <c>AwardPointsAsync</c> correctly triggers a <see cref="LevelUpEvent"/>
/// when the player's XP crosses a level threshold.
/// </summary>
public sealed class AwardPointsLevelUpTests
{
    [Fact]
    public async Task AwardPoints_CrossesCertifiedThreshold_RaisesLevelUpEvent()
    {
        // Arrange — Certified requires 200 XP.
        // Give the player 195 XP first via a custom override, then push them over.
        var (svc, _, mediator) = ServiceBuilder.Build();
        await svc.EnsurePlayerAsync("user-1", "Alice");

        await svc.AwardPointsAsync("user-1", GamificationActions.JobAccepted, overridePoints: 195);
        mediator.Published.Clear(); // reset observation window

        // Act — one QC approval (20 XP) takes total to 215 → should cross 200 threshold
        await svc.AwardPointsAsync("user-1", GamificationActions.QcApproved);

        // Assert
        var levelUp = mediator.Published.OfType<LevelUpEvent>().SingleOrDefault();
        levelUp.Should().NotBeNull();
        levelUp!.OldLevel.Should().Be(1);
        levelUp.NewLevel.Should().Be(2);
        levelUp.NewLevelName.Should().Be("Certified");
        levelUp.ExternalUserId.Should().Be("user-1");
    }

    [Fact]
    public async Task AwardPoints_BelowThreshold_DoesNotRaiseLevelUpEvent()
    {
        var (svc, _, mediator) = ServiceBuilder.Build();
        await svc.EnsurePlayerAsync("user-2", "Bob");

        // 5 XP — still well below Certified threshold of 200.
        await svc.AwardPointsAsync("user-2", GamificationActions.JobAccepted);

        mediator.Published.OfType<LevelUpEvent>().Should().BeEmpty();
    }

    [Fact]
    public async Task AwardPoints_CrossesExpertThreshold_RaisesLevelUpEvent()
    {
        // Expert = 700 XP.
        var (svc, _, mediator) = ServiceBuilder.Build();
        await svc.EnsurePlayerAsync("user-3", "Carol");

        await svc.AwardPointsAsync("user-3", GamificationActions.JobAccepted, overridePoints: 690);
        mediator.Published.Clear();

        // Act — 20 XP QC crosses 700
        await svc.AwardPointsAsync("user-3", GamificationActions.QcApproved);

        var levelUp = mediator.Published.OfType<LevelUpEvent>().SingleOrDefault();
        levelUp.Should().NotBeNull();
        levelUp!.NewLevel.Should().Be(3);
        levelUp.NewLevelName.Should().Be("Expert");
    }

    [Fact]
    public async Task AwardPoints_NoXPAction_DoesNotRaiseAnyEvents()
    {
        var (svc, _, mediator) = ServiceBuilder.Build();
        await svc.EnsurePlayerAsync("user-4", "Dave");

        // A QCRejected is not in the XP map, so 0 XP is awarded.
        await svc.AwardPointsAsync("user-4", GamificationActions.QcRejected);

        mediator.Published.OfType<PointsAwardedEvent>().Should().BeEmpty();
        mediator.Published.OfType<LevelUpEvent>().Should().BeEmpty();
    }

    [Fact]
    public async Task AwardPoints_MultipleAwards_AccumulatesXpCorrectly()
    {
        var (svc, db, _) = ServiceBuilder.Build();
        await svc.EnsurePlayerAsync("user-5", "Eve");

        await svc.AwardPointsAsync("user-5", GamificationActions.JobAccepted);      // +5
        await svc.AwardPointsAsync("user-5", GamificationActions.QcApproved);       // +20
        await svc.AwardPointsAsync("user-5", GamificationActions.OrderDelivered);   // +10

        var player = await svc.GetPlayerAsync("user-5");
        player!.TotalXP.Should().Be(35);
    }

    [Fact]
    public async Task AwardPoints_PointTransactionIsAppendOnly()
    {
        var (svc, db, _) = ServiceBuilder.Build();
        await svc.EnsurePlayerAsync("user-6", "Frank");

        await svc.AwardPointsAsync("user-6", GamificationActions.JobAccepted);
        await svc.AwardPointsAsync("user-6", GamificationActions.QcApproved);

        var txCount = db.Transactions.Count(t => t.Player.ExternalUserId == "user-6");
        txCount.Should().Be(2, "each award creates exactly one transaction row");
    }

    [Fact]
    public async Task AwardPoints_LevelUpEventContainsCorrectTotalXP()
    {
        var (svc, _, mediator) = ServiceBuilder.Build();
        await svc.EnsurePlayerAsync("user-7", "Grace");

        // Push just over Certified (200 XP).
        await svc.AwardPointsAsync("user-7", GamificationActions.ProfileCompleted, overridePoints: 205);

        var levelUp = mediator.Published.OfType<LevelUpEvent>().Single();
        levelUp.TotalXP.Should().Be(205);
    }
}
