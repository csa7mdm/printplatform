using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Abstractions;

/// <summary>
/// Primary entry-point for the host platform to interact with the loyalty engine.
/// All operations are keyed on <paramref name="externalUserId"/> — no domain entity is required.
/// </summary>
public interface ILoyaltyService
{
    /// <summary>
    /// Looks up an existing member or creates a new enrolment record.
    /// Idempotent — safe to call on every login or first action.
    /// </summary>
    /// <param name="externalUserId">Opaque host-platform user identifier.</param>
    /// <param name="dateOfBirth">Optional DOB used for birthday multiplier evaluation.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<LoyaltyMember> EnsureMemberAsync(
        string externalUserId,
        DateOnly? dateOfBirth = null,
        CancellationToken ct = default);

    /// <summary>
    /// Awards points for a completed order.
    /// Applies tier multiplier, birthday multiplier, design-service multiplier, and first-order bonus.
    /// Raises <c>PointsEarnedEvent</c> and, if the tier changes, <c>TierUpgradedEvent</c> /
    /// <c>TierDowngradedEvent</c>.
    /// </summary>
    /// <param name="externalUserId">Member to credit.</param>
    /// <param name="orderAmountEgp">Total order value in Egyptian Pounds.</param>
    /// <param name="orderReference">Host-platform order identifier (stored on ledger row).</param>
    /// <param name="hasDesignService">Whether a design-service upsell was included.</param>
    /// <param name="bonusPoints">Optional fixed bonus points to award.</param>
    /// <param name="reason">Optional reason for awarding points (e.g. "ReviewBonus").</param>
    /// <param name="ct">Cancellation token.</param>
    Task<EarnResult> EarnAsync(
        string externalUserId,
        decimal orderAmountEgp,
        string? orderReference = null,
        bool hasDesignService = false,
        int? bonusPoints = null,
        string? reason = null,
        CancellationToken ct = default);

    /// <summary>
    /// Redeems a reward, deducting points from the member's active balance.
    /// Raises <c>RewardRedeemedEvent</c>.
    /// Returns a <see cref="RedemptionResult"/> describing the discount / credit to apply.
    /// Throws <see cref="InsufficientPointsException"/> if balance is too low.
    /// </summary>
    Task<RedemptionResult> RedeemAsync(
        string externalUserId,
        long rewardId,
        string? orderReference = null,
        CancellationToken ct = default);

    /// <summary>Returns the current snapshot for a member, or <c>null</c> if not found.</summary>
    Task<LoyaltyMember?> GetMemberAsync(string externalUserId, CancellationToken ct = default);

    /// <summary>Returns all ledger entries for a member, newest first.</summary>
    Task<IReadOnlyList<PointsLedger>> GetLedgerAsync(
        string externalUserId,
        int pageSize = 50,
        int page = 0,
        CancellationToken ct = default);

    /// <summary>Returns all active (non-expired) rewards available for redemption.</summary>
    Task<IReadOnlyList<Reward>> GetRewardsAsync(CancellationToken ct = default);

    /// <summary>Returns all redemptions for a member, newest first.</summary>
    Task<IReadOnlyList<Redemption>> GetRedemptionsAsync(
        string externalUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Processes point expirations for all members, deducting points that have passed their expiry date.
    /// Returns the total number of points expired across all members.
    /// </summary>
    Task<int> ProcessExpiriesAsync(CancellationToken ct = default);
}

/// <summary>Result returned by <see cref="ILoyaltyService.EarnAsync"/>.</summary>
public sealed record EarnResult(
    string ExternalUserId,
    int BasePoints,
    int BonusPoints,
    int TotalAwarded,
    int NewActivePoints,
    int NewLifetimePoints,
    string? NewTierName);

/// <summary>Result returned by <see cref="ILoyaltyService.RedeemAsync"/>.</summary>
public sealed record RedemptionResult(
    long RedemptionId,
    string RewardName,
    RewardType RewardType,
    double RewardValue,
    int PointsUsed,
    int RemainingActivePoints);

/// <summary>
/// Thrown by <see cref="ILoyaltyService.RedeemAsync"/> when the member's active
/// balance is below the reward's point cost.
/// </summary>
public sealed class InsufficientPointsException : Exception
{
    public int Required { get; }
    public int Available { get; }

    public InsufficientPointsException(int required, int available)
        : base($"Insufficient points: required {required}, available {available}.")
    {
        Required  = required;
        Available = available;
    }
}
