using MediatR;
using PrintPlatform.Application.Events;
using PrintPlatform.Domain.Dispatch.Events;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Gamification;
using PrintPlatform.Gamification.Abstractions;

namespace PrintPlatform.Application.EventHandlers;

public sealed class GamificationEventHandler :
    INotificationHandler<DomainEventNotification<JobAcceptedEvent>>,
    INotificationHandler<DomainEventNotification<QCApprovedEvent>>,
    INotificationHandler<DomainEventNotification<QCRejectedEvent>>,
    INotificationHandler<DomainEventNotification<JobDeliveredEvent>>,
    INotificationHandler<DomainEventNotification<ReviewReceivedEvent>>
{
    private readonly IGamificationService _gamificationService;

    public GamificationEventHandler(IGamificationService gamificationService)
    {
        _gamificationService = gamificationService;
    }

    public Task Handle(DomainEventNotification<JobAcceptedEvent> notification, CancellationToken ct)
        => _gamificationService.AwardPointsAsync(notification.DomainEvent.PrinterOwnerUserId.ToString(), GamificationActions.JobAccepted, 5, ct: ct);

    public Task Handle(DomainEventNotification<QCApprovedEvent> notification, CancellationToken ct)
        => _gamificationService.AwardPointsAsync(notification.DomainEvent.PrinterOwnerUserId.ToString(), GamificationActions.QcApproved, 20, ct: ct);

    public Task Handle(DomainEventNotification<QCRejectedEvent> notification, CancellationToken ct)
        => _gamificationService.AwardPointsAsync(notification.DomainEvent.PrinterOwnerUserId.ToString(), GamificationActions.QcRejected, 0, ct: ct);

    public Task Handle(DomainEventNotification<JobDeliveredEvent> notification, CancellationToken ct)
        => _gamificationService.AwardPointsAsync(notification.DomainEvent.PrinterOwnerUserId.ToString(), GamificationActions.OrderDelivered, 10, ct: ct);

    public Task Handle(DomainEventNotification<ReviewReceivedEvent> notification, CancellationToken ct)
        => _gamificationService.AwardPointsAsync(notification.DomainEvent.PrinterOwnerUserId.ToString(), GamificationActions.ReviewReceived, 5, ct: ct);
}
