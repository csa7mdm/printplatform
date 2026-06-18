using MediatR;

namespace PrintPlatform.Loyalty.Events;

/// <summary>
/// Published when a member's tier advances to a higher level.
/// </summary>
public sealed record TierUpgradedEvent(
    string UserId,
    string OldTier,
    string NewTier,
    int CurrentLifetimePoints,
    DateTimeOffset OccurredAt) : INotification;
