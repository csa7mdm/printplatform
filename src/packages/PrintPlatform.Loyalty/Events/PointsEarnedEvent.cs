using MediatR;

namespace PrintPlatform.Loyalty.Events;

/// <summary>
/// Published after points are successfully written to the ledger and the member's
/// active balance is updated. Consumed by host handlers for notifications, analytics, etc.
/// </summary>
public sealed record PointsEarnedEvent(
    string ExternalUserId,
    int PointsAwarded,
    int NewActivePoints,
    int NewLifetimePoints,
    string? OrderReference,
    DateTimeOffset OccurredAt) : INotification;
