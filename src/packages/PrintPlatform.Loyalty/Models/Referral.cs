namespace PrintPlatform.Loyalty.Models;

/// <summary>
/// Tracks a referral relationship between two members.
/// Bonus points are awarded to the referrer only once the referee completes their first order.
/// </summary>
public sealed class Referral
{
    /// <summary>Surrogate PK.</summary>
    public long Id { get; set; }

    /// <summary>ExternalUserId of the person who shared the referral code.</summary>
    public string ReferrerId { get; set; } = string.Empty;

    /// <summary>ExternalUserId of the person who signed up using the referral code.</summary>
    public string ReferredUserId { get; set; } = string.Empty;

    /// <summary>The unique referral code that was used.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Lifecycle status of the referral.</summary>
    public ReferralStatus Status { get; set; } = ReferralStatus.Pending;

    /// <summary>
    /// Points credited to the referrer after conversion.
    /// Populated when <see cref="Status"/> transitions to <see cref="ReferralStatus.Converted"/>.
    /// </summary>
    public int BonusPoints { get; set; }

    /// <summary>UTC timestamp the referral record was created (referee signed up).</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>UTC timestamp the referral converted (referee's first order completed).</summary>
    public DateTimeOffset? ConvertedAt { get; set; }
}

/// <summary>Lifecycle state of a referral.</summary>
public enum ReferralStatus
{
    /// <summary>Referee has signed up but has not yet placed their first order.</summary>
    Pending = 1,

    /// <summary>Referee completed their first order; bonus was awarded to referrer.</summary>
    Converted = 2,

    /// <summary>Referral was voided (e.g. fraud detection, duplicate account).</summary>
    Voided = 3,
}
