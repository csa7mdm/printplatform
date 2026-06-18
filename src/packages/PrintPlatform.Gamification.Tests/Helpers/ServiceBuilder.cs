using Microsoft.Extensions.Options;
using PrintPlatform.Gamification.Abstractions;
using PrintPlatform.Gamification.Data;
using PrintPlatform.Gamification.Services;

namespace PrintPlatform.Gamification.Tests.Helpers;

/// <summary>Wires up the full service graph with an in-memory DB and a fake mediator.</summary>
internal static class ServiceBuilder
{
    /// <summary>Creates everything from scratch with a new in-memory DB.</summary>
    public static (GamificationService Svc, GamificationDbContext Db, FakeMediator Mediator)
        Build(
            GamificationOptions? options = null,
            IEnumerable<IAchievementRule>? rules = null,
            IEnumerable<ILeaderboardProvider>? leaderboardProviders = null)
    {
        var db       = GamificationDbContextFactory.Create();
        var mediator = new FakeMediator();
        var svc      = BuildService(db, mediator, options, rules, leaderboardProviders);
        return (svc, db, mediator);
    }

    /// <summary>
    /// Builds the service graph using a caller-supplied <paramref name="db"/> and
    /// <paramref name="mediator"/>, allowing rules that need to close over <paramref name="db"/>
    /// to be constructed before calling this method.
    /// </summary>
    public static (GamificationService Svc, GamificationDbContext Db, FakeMediator Mediator)
        BuildWithDb(
            GamificationDbContext db,
            FakeMediator mediator,
            GamificationOptions? options = null,
            IEnumerable<IAchievementRule>? rules = null,
            IEnumerable<ILeaderboardProvider>? leaderboardProviders = null)
    {
        var svc = BuildService(db, mediator, options, rules, leaderboardProviders);
        return (svc, db, mediator);
    }

    private static GamificationService BuildService(
        GamificationDbContext db,
        FakeMediator mediator,
        GamificationOptions? options,
        IEnumerable<IAchievementRule>? rules,
        IEnumerable<ILeaderboardProvider>? leaderboardProviders)
    {
        var opts         = Options.Create(options ?? new GamificationOptions());
        var ruleList     = rules ?? [];
        var providerList = leaderboardProviders ?? [];

        var achievementEngine  = new AchievementEngine(db, ruleList, mediator);
        var streakService      = new StreakService(db, mediator, opts);
        var leaderboardService = new LeaderboardService(db, providerList, opts);

        return new GamificationService(
            db, mediator, opts, achievementEngine, streakService, leaderboardService);
    }
}
