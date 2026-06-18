using MediatR;

namespace PrintPlatform.Loyalty.Events;

/// <summary>
/// Published when a referred user places their first order, triggering the bonus
/// credit to the referrer.
/// </summary>
public sealed record ReferralConvertedEvent(
    string ReferrerId,
    string ReferredUserId,
    string ReferralCode,
    int BonusPointsAwarded,
    DateTimeOffset OccurredAt) : INotification;
