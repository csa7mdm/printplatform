using MediatR;
using Microsoft.Extensions.Logging;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Application.Finance;

namespace PrintPlatform.Infrastructure.Finance;

public sealed class WeeklyPayoutBatchJob
{
    private readonly ISender _sender;
    private readonly INotificationService _notifications;
    private readonly ILogger<WeeklyPayoutBatchJob> _logger;

    public WeeklyPayoutBatchJob(
        ISender sender,
        INotificationService notifications,
        ILogger<WeeklyPayoutBatchJob> logger)
    {
        _sender = sender;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var periodEnd = now.Date;
        var periodStart = periodEnd.AddDays(-7);

        var result = await _sender.Send(new InitiateWeeklyPayoutsCommand(periodStart, periodEnd), ct);
        if (result.IsFailure)
        {
            _logger.LogInformation(
                "Weekly payout batch skipped or failed: {Code} {Message}",
                result.Error.Code,
                result.Error.Message);
            return;
        }

        foreach (var payout in result.Value)
        {
            var notify = await _notifications.SendPayoutProcessed(
                payout.PrinterOwnerUserId,
                payout.NetAmountEgp,
                ct);

            if (notify.IsFailure)
            {
                _logger.LogWarning(
                    "Payout notification failed for payout {PayoutId}: {Code}",
                    payout.Id,
                    notify.Error.Code);
            }
        }

        _logger.LogInformation(
            "Weekly payout batch created {PayoutCount} payouts totaling {TotalAmountEgp} EGP.",
            result.Value.Count,
            result.Value.Sum(p => p.NetAmountEgp));
    }
}
