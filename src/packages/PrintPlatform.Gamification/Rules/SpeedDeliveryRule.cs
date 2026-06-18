using System.Text.Json;
using PrintPlatform.Gamification.Abstractions;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Rules;

/// <summary>
/// "Speed Demon" badge — awarded when an order is delivered within 24 hours of acceptance.
/// The host must supply a JSON metadata bag containing <c>acceptedAt</c> (ISO-8601 string)
/// alongside the <c>OrderDelivered</c> action for this rule to evaluate correctly.
/// <example>
/// <code>
/// await gamification.AwardPointsAsync(userId, GamificationActions.OrderDelivered,
///     metadataJson: """{"acceptedAt":"2024-01-15T08:00:00Z"}""");
/// </code>
/// </example>
/// </summary>
public sealed class SpeedDeliveryRule : IAchievementRule
{
    public const long SpeedDemonAchievementId = 20L;

    private static readonly TimeSpan DeliveryWindow = TimeSpan.FromHours(24);

    /// <inheritdoc />
    public long AchievementId => SpeedDemonAchievementId;

    /// <inheritdoc />
    public Task<bool> IsSatisfiedAsync(
        GamificationPlayer player,
        AchievementContext context,
        CancellationToken ct = default)
    {
        if (context.Action != GamificationActions.OrderDelivered)
            return Task.FromResult(false);

        if (string.IsNullOrWhiteSpace(context.MetadataJson))
            return Task.FromResult(false);

        try
        {
            using var doc = JsonDocument.Parse(context.MetadataJson);
            if (!doc.RootElement.TryGetProperty("acceptedAt", out var acceptedAtEl))
                return Task.FromResult(false);

            if (!acceptedAtEl.TryGetDateTimeOffset(out var acceptedAt))
                return Task.FromResult(false);

            var elapsed = context.OccurredAt - acceptedAt;
            return Task.FromResult(elapsed > TimeSpan.Zero && elapsed <= DeliveryWindow);
        }
        catch (JsonException)
        {
            return Task.FromResult(false);
        }
    }
}
