using MediatR;
using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Events;

/// <summary>
/// Published after a reward redemption is persisted.
/// Carries enough information for the host to apply the discount / credit to an order.
/// </summary>
public sealed record RewardRedeemedEvent(
    string ExternalUserId,
    long RedemptionId,
    long RewardId,
    string RewardName,
    RewardType RewardType,
    double RewardValue,
    int PointsUsed,
    string? OrderReference,
    DateTimeOffset OccurredAt) : INotification;
