using MediatR;

namespace PrintPlatform.Gamification.Events;

/// <summary>
/// Published after XP is successfully written to the database and the player's
/// <c>TotalXP</c> is updated. Consumed by host handlers to update dashboards,
/// send push notifications, etc.
/// </summary>
/// <param name="ExternalUserId">Opaque host-platform user identifier.</param>
/// <param name="Points">XP amount that was awarded.</param>
/// <param name="Action">Action string that triggered the award.</param>
/// <param name="NewTotal">Player's total XP after this award.</param>
/// <param name="MetadataJson">Optional host-supplied JSON context.</param>
public sealed record PointsAwardedEvent(
    string ExternalUserId,
    int Points,
    string Action,
    int NewTotal,
    string? MetadataJson) : INotification;
