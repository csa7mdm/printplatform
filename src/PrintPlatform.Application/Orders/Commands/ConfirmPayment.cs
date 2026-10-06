using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Orders.Commands;

/// <summary>
/// Processes a verified Paymob payment webhook. The HMAC must already be validated by
/// the caller (controller) before this command runs. On a successful, captured
/// transaction the matching order is transitioned to <see cref="OrderStatus.Confirmed"/>.
/// Idempotent: replayed webhooks for an already-confirmed order succeed without change.
/// </summary>
public sealed record ConfirmPaymentCommand(
    string PaymobOrderId,
    bool Success,
    decimal AmountCents) : IRequest<Result>;

public sealed class ConfirmPaymentHandler : IRequestHandler<ConfirmPaymentCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly ILogger<ConfirmPaymentHandler> _logger;

    public ConfirmPaymentHandler(IAppDbContext db, ILogger<ConfirmPaymentHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.PaymobOrderId == request.PaymobOrderId, cancellationToken);
        if (order is null)
        {
            _logger.LogWarning(
                "Paymob webhook for unknown Paymob order {PaymobOrderId}", request.PaymobOrderId);
            return Result.Failure(OrderErrors.OrderNotFound);
        }

        if (!request.Success)
        {
            _logger.LogInformation(
                "Paymob reported a failed/declined transaction for order {OrderId}", order.Id);
            return Result.Success(); // leave order PendingPayment; customer may retry.
        }

        // Amount reconciliation: Paymob "amount_cents" is piasters (EGP × 100).
        var expectedCents = decimal.Round(order.TotalEgp * 100m, 0);
        if (request.AmountCents > 0m && request.AmountCents != expectedCents)
        {
            _logger.LogWarning(
                "Paymob amount mismatch for order {OrderId}: expected {Expected} piasters, got {Actual}",
                order.Id, expectedCents, request.AmountCents);
            return Result.Failure(OrderErrors.PaymentAmountMismatch);
        }

        var confirm = order.Confirm();
        if (confirm.IsFailure)
            return confirm;

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Order {OrderId} confirmed via Paymob webhook", order.Id);
        return Result.Success();
    }
}