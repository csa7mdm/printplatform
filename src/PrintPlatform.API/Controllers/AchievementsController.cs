using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PrintPlatform.Gamification;
using PrintPlatform.Gamification.Abstractions;

namespace PrintPlatform.API.Controllers;

[Authorize]
[Route("api/achievements")]
[Produces("application/json")]
public sealed class AchievementsController : ApiControllerBase
{
    private readonly IGamificationService _gamification;
    private readonly GamificationOptions _options;

    public AchievementsController(
        IGamificationService gamification,
        IOptions<GamificationOptions> options)
    {
        _gamification = gamification;
        _options = options.Value;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(AchievementsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var externalUserId = CurrentUserId.ToString();

        var player = await _gamification.GetPlayerAsync(externalUserId, ct);
        var achievements = await _gamification.GetPlayerAchievementsAsync(externalUserId, ct);
        var streaks = await _gamification.GetStreaksAsync(externalUserId, ct);

        var totalXp = player?.TotalXP ?? 0;
        var levelDefinition = _options.Levels.FirstOrDefault(level => level.Tier == (player?.CurrentLevel ?? 1))
            ?? _options.ResolveLevel(totalXp);
        var nextLevel = _options.Levels
            .OrderBy(level => level.MinXP)
            .FirstOrDefault(level => level.MinXP > totalXp);
        var currentStreak = streaks
            .OrderByDescending(streak => streak.CurrentCount)
            .ThenByDescending(streak => streak.LastActionDate)
            .FirstOrDefault();

        var response = new AchievementsResponse(
            levelDefinition.Tier,
            levelDefinition.Name,
            totalXp,
            nextLevel?.MinXP,
            achievements.Select(achievement => new AchievementBadgeResponse(
                achievement.AchievementId,
                achievement.Achievement.Name,
                achievement.Achievement.Description,
                achievement.Achievement.IconSlug,
                achievement.Achievement.Category,
                achievement.Achievement.BonusXP,
                achievement.UnlockedAt)).ToArray(),
            currentStreak is null
                ? null
                : new CurrentStreakResponse(
                    currentStreak.ActionType,
                    currentStreak.CurrentCount,
                    currentStreak.BestCount,
                    currentStreak.LastActionDate));

        return Ok(response);
    }
}

public sealed record AchievementsResponse(
    int Level,
    string LevelName,
    int TotalXp,
    int? NextLevelXp,
    IReadOnlyList<AchievementBadgeResponse> Badges,
    CurrentStreakResponse? CurrentStreak);

public sealed record AchievementBadgeResponse(
    long Id,
    string Name,
    string Description,
    string IconSlug,
    string Category,
    int BonusXp,
    DateTimeOffset UnlockedAt);

public sealed record CurrentStreakResponse(
    string ActionType,
    int CurrentCount,
    int BestCount,
    DateTimeOffset LastActionDate);
