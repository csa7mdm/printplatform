using MediatR;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Gamification.Data;
using PrintPlatform.Gamification.Events;

namespace PrintPlatform.Application.EventHandlers;

public sealed class GamificationNotificationHandler :
    INotificationHandler<AchievementUnlockedEvent>,
    INotificationHandler<LevelUpEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdentityRepository _identityRepository;
    private readonly GamificationDbContext _gamificationDb;

    public GamificationNotificationHandler(
        INotificationService notificationService,
        IIdentityRepository identityRepository,
        GamificationDbContext gamificationDb)
    {
        _notificationService = notificationService;
        _identityRepository = identityRepository;
        _gamificationDb = gamificationDb;
    }

    public async Task Handle(AchievementUnlockedEvent notification, CancellationToken ct)
    {
        if (!Guid.TryParse(notification.ExternalUserId, out var userId)) return;

        var user = await _identityRepository.GetUserByIdAsync(userId, ct);
        if (user is null) return;

        var achievement = await _gamificationDb.Achievements.FindAsync(new object[] { notification.AchievementId }, ct);
        var description = achievement?.Description ?? notification.AchievementName;

        await _notificationService.SendAchievementUnlocked(user.PhoneNumber, notification.AchievementName, description);
    }

    public async Task Handle(LevelUpEvent notification, CancellationToken ct)
    {
        if (!Guid.TryParse(notification.ExternalUserId, out var userId)) return;

        var user = await _identityRepository.GetUserByIdAsync(userId, ct);
        if (user is null) return;

        await _notificationService.SendLevelUp(user.PhoneNumber, notification.NewLevel);
    }
}
