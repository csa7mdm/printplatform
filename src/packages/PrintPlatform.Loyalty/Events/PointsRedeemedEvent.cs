using MediatR;

namespace PrintPlatform.Loyalty.Events;

/// <summary>
/// Published after a member's points balance is reduced by a redemption
/// (before <see cref="RewardRedeemedEvent"/> which carries the reward details).
/// </summary>
public sealed record PointsRedeemedEvent(
    string ExternalUserId,
    int PointsDeducted,
    int RemainingActivePoints,
    string? OrderReference,
    DateTimeOffset OccurredAt) : INotification;
