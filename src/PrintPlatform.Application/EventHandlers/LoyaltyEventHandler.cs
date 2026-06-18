using MediatR;
using PrintPlatform.Application.Events;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Loyalty.Abstractions;

namespace PrintPlatform.Application.EventHandlers;

public sealed class LoyaltyEventHandler :
    INotificationHandler<DomainEventNotification<OrderConfirmedEvent>>,
    INotificationHandler<DomainEventNotification<ReviewLeftEvent>>
{
    private readonly ILoyaltyService _loyaltyService;

    public LoyaltyEventHandler(ILoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    public Task Handle(DomainEventNotification<OrderConfirmedEvent> notification, CancellationToken ct)
        => _loyaltyService.EarnAsync(
            notification.DomainEvent.CustomerUserId.ToString(),
            notification.DomainEvent.TotalEgp,
            notification.DomainEvent.OrderId.ToString(),
            ct: ct);

    public Task Handle(DomainEventNotification<ReviewLeftEvent> notification, CancellationToken ct)
        => _loyaltyService.EarnAsync(
            notification.DomainEvent.CustomerUserId.ToString(),
            0,
            notification.DomainEvent.OrderId.ToString(),
            bonusPoints: 50,
            reason: "ReviewBonus",
            ct: ct);
}
