namespace PrintPlatform.Gamification.Models;

/// <summary>
/// Tracks a player's progress toward completing a <see cref="Challenge"/>.
/// </summary>
public sealed class PlayerChallenge
{
    public long PlayerId     { get; set; }
    public long ChallengeId  { get; set; }

    /// <summary>
    /// Numeric progress counter (semantics defined by <see cref="Challenge.CriteriaJson"/>).
    /// E.g. number of deliveries made during the challenge window.
    /// </summary>
    public int Progress { get; set; }

    /// <summary>UTC timestamp when the challenge was completed; <c>null</c> if in-progress.</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    // Navigation
    public GamificationPlayer Player    { get; set; } = null!;
    public Challenge           Challenge { get; set; } = null!;
}
