namespace PrintPlatform.Gamification;

/// <summary>
/// Strongly-typed options for the Gamification engine.
/// Bound from <c>appsettings.json → "Gamification"</c> or configured via
/// <c>services.AddGamification(o => ...)</c>.
/// </summary>
public sealed class GamificationOptions
{
    public const string SectionName = "Gamification";

    // ── XP action map ────────────────────────────────────────────────────────

    /// <summary>XP awarded when a player accepts a job.</summary>
    public int XpJobAccepted { get; set; } = 5;

    /// <summary>XP awarded when a quality-control check is approved.</summary>
    public int XpQcApproved { get; set; } = 20;

    /// <summary>XP awarded when an order is delivered to the customer.</summary>
    public int XpOrderDelivered { get; set; } = 10;

    /// <summary>XP awarded when a player completes their profile for the first time.</summary>
    public int XpProfileCompleted { get; set; } = 50;

    /// <summary>XP awarded for completing the very first job.</summary>
    public int XpFirstJobCompleted { get; set; } = 100;

    /// <summary>XP awarded each time a review is received.</summary>
    public int XpReviewReceived { get; set; } = 5;

    // ── Multiplier ────────────────────────────────────────────────────────────

    /// <summary>Global multiplier applied to every XP award (e.g. 2.0 during events).</summary>
    public double BonusMultiplier { get; set; } = 1.0;

    // ── Streak ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Maximum gap in hours between same-action events before a streak is broken.
    /// Default 48 h so a player can miss one day without losing their streak.
    /// </summary>
    public int StreakBreakThresholdHours { get; set; } = 48;

    /// <summary>Streak milestones that trigger a <c>StreakMilestoneEvent</c>.</summary>
    public int[] StreakMilestones { get; set; } = [3, 7, 14, 30, 100];

    // ── Leaderboard ───────────────────────────────────────────────────────────

    /// <summary>How many top entries to keep in a leaderboard snapshot.</summary>
    public int LeaderboardTopN { get; set; } = 100;

    // ── Level thresholds (ordered ascending) ─────────────────────────────────

    /// <summary>
    /// Default level definitions for the 3-D printing platform.
    /// The host can replace the entire list to customise progression.
    /// </summary>
    public LevelDefinition[] Levels { get; set; } =
    [
        new(1, "Novice",     0,    199,  """{"perks":["listed_in_network"]}"""),
        new(2, "Certified",  200,  699,  """{"perks":["priority_dispatch_queue"]}"""),
        new(3, "Expert",     700,  1999, """{"perks":["fee_reduction_1pct"]}"""),
        new(4, "Master",     2000, 4999, """{"perks":["featured_on_platform"]}"""),
        new(5, "Elite",      5000, int.MaxValue, """{"perks":["white_glove_support","custom_sla_badge"]}"""),
    ];

    // ── Expiry ────────────────────────────────────────────────────────────────

    /// <summary>Days before unspent points expire (0 = never).</summary>
    public int PointsExpiryDays { get; set; } = 0;

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the configured XP for a known action string.
    /// Returns <c>0</c> for unrecognised actions (challenges override via <see cref="Challenge.RewardXP"/>).
    /// </summary>
    public int ResolveXp(string action) => action switch
    {
        GamificationActions.JobAccepted       => (int)(XpJobAccepted       * BonusMultiplier),
        GamificationActions.QcApproved        => (int)(XpQcApproved        * BonusMultiplier),
        GamificationActions.OrderDelivered    => (int)(XpOrderDelivered    * BonusMultiplier),
        GamificationActions.ProfileCompleted  => (int)(XpProfileCompleted  * BonusMultiplier),
        GamificationActions.FirstJobCompleted => (int)(XpFirstJobCompleted * BonusMultiplier),
        GamificationActions.ReviewReceived    => (int)(XpReviewReceived    * BonusMultiplier),
        _                                      => 0,
    };

    /// <summary>
    /// Returns the <see cref="LevelDefinition"/> for a given total XP, or the highest tier if XP exceeds all.
    /// </summary>
    public LevelDefinition ResolveLevel(int totalXp)
    {
        LevelDefinition result = Levels[0];
        foreach (var lvl in Levels)
        {
            if (totalXp >= lvl.MinXP)
                result = lvl;
        }
        return result;
    }
}

/// <summary>Immutable level definition supplied via <see cref="GamificationOptions.Levels"/>.</summary>
public sealed record LevelDefinition(int Tier, string Name, int MinXP, int MaxXP, string BenefitsJson);
