using MediatR;

namespace PrintPlatform.Loyalty.Events;

/// <summary>
/// Published when a member's tier drops to a lower level.
/// Tier downgrade can occur when active points fall below the minimum threshold
/// of the current tier (e.g. after expiry or in custom strategies).
/// </summary>
public sealed record TierDowngradedEvent(
    string UserId,
    string OldTier,
    string NewTier,
    int CurrentLifetimePoints,
    DateTimeOffset OccurredAt) : INotification;
