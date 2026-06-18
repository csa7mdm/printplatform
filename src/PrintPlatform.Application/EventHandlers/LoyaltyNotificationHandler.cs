using MediatR;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Loyalty.Events;

namespace PrintPlatform.Application.EventHandlers;

public sealed class LoyaltyNotificationHandler :
    INotificationHandler<TierUpgradedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IIdentityRepository _identityRepository;

    public LoyaltyNotificationHandler(
        INotificationService notificationService,
        IIdentityRepository identityRepository)
    {
        _notificationService = notificationService;
        _identityRepository = identityRepository;
    }

    public async Task Handle(TierUpgradedEvent notification, CancellationToken ct)
    {
        if (!Guid.TryParse(notification.UserId, out var userId)) return;

        var user = await _identityRepository.GetUserByIdAsync(userId, ct);
        if (user is null) return;

        await _notificationService.SendTierUpgrade(user.PhoneNumber, notification.NewTier);
    }
}
