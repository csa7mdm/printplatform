namespace PrintPlatform.Gamification.Models;

/// <summary>
/// Defines an achievement that can be unlocked by a player.
/// Criteria are evaluated by <see cref="Rules.IAchievementRule"/> implementations.
/// </summary>
public sealed class Achievement
{
    public long Id { get; set; }

    /// <summary>Human-readable name shown in the UI (e.g. "Perfectionist").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short description explaining how to unlock this achievement.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Slug used to resolve the icon asset (e.g. "badge-perfectionist").</summary>
    public string IconSlug { get; set; } = string.Empty;

    /// <summary>
    /// JSON document storing rule-specific parameters interpreted by the matching
    /// <c>IAchievementRule</c> (e.g. <c>{"count":10,"action":"QCApproved"}</c>).
    /// </summary>
    public string CriteriaJson { get; set; } = "{}";

    /// <summary>Grouping for display purposes (e.g. "Quality", "Milestones", "Speed").</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>XP bonus awarded when this achievement is first unlocked (may be 0).</summary>
    public int BonusXP { get; set; }

    public ICollection<PlayerAchievement> PlayerAchievements { get; set; } = [];
}
