namespace PrintPlatform.Gamification.Models;

/// <summary>
/// A time-boxed challenge that players can participate in for bonus XP rewards.
/// </summary>
public sealed class Challenge
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// JSON document describing the completion criteria
    /// (e.g. <c>{"action":"OrderDelivered","targetCount":5}</c>).
    /// </summary>
    public string CriteriaJson { get; set; } = "{}";

    /// <summary>XP awarded on challenge completion.</summary>
    public int RewardXP { get; set; }

    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate   { get; set; }

    /// <summary>Whether the challenge is published and visible to players.</summary>
    public bool IsActive { get; set; } = true;

    public ICollection<PlayerChallenge> PlayerChallenges { get; set; } = [];
}
