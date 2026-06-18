using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Orders.Commands;

/// <summary>
/// Initiates a Paymob payment for a PendingPayment order and returns the iframe URL /
/// payment key the client uses to complete the card payment. Persists the returned
/// Paymob order id on the order so the webhook can be correlated.
/// </summary>
public sealed record InitiatePaymentCommand(
    Guid OrderId,
    Guid CustomerUserId,
    string CustomerFirstName,
    string CustomerLastName,
    string CustomerEmail,
    string CustomerPhone) : IRequest<Result<InitiatePaymentResponse>>;

public sealed class InitiatePaymentHandler
    : IRequestHandler<InitiatePaymentCommand, Result<InitiatePaymentResponse>>
{
    private readonly IAppDbContext _db;
    private readonly IPaymentGateway _gateway;
    private readonly ILogger<InitiatePaymentHandler> _logger;

    public InitiatePaymentHandler(
        IAppDbContext db, IPaymentGateway gateway, ILogger<InitiatePaymentHandler> logger)
    {
        _db = db;
        _gateway = gateway;
        _logger = logger;
    }

    public async Task<Result<InitiatePaymentResponse>> Handle(
        InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<InitiatePaymentResponse>(OrderErrors.OrderNotFound);

        if (order.CustomerUserId != request.CustomerUserId)
            return Result.Failure<InitiatePaymentResponse>(OrderErrors.OrderNotFound);

        if (order.Status != OrderStatus.PendingPayment)
            return Result.Failure<InitiatePaymentResponse>(OrderErrors.OrderNotPendingPayment);

        var address = DeserializeAddress(order.ShippingAddressSnapshot);

        try
        {
            var result = await _gateway.InitiatePaymentAsync(
                new InitiatePaymentRequest(
                    order.Id,
                    order.TotalEgp,
                    request.CustomerFirstName,
                    request.CustomerLastName,
                    request.CustomerEmail,
                    request.CustomerPhone,
                    address?.Street ?? "NA",
                    address?.City ?? "NA",
                    address?.Country ?? "Egypt"),
                cancellationToken);

            order.AttachPaymobOrder(result.PaymobOrderId);
            await _db.SaveChangesAsync(cancellationToken);

            return new InitiatePaymentResponse(result.PaymobOrderId, result.PaymentKey, result.IframeUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paymob initiation failed for order {OrderId}", order.Id);
            return Result.Failure<InitiatePaymentResponse>(OrderErrors.PaymentInitiationFailed);
        }
    }

    private static ShippingAddress? DeserializeAddress(string json)
    {
        try { return JsonSerializer.Deserialize<ShippingAddress>(json); }
        catch { return null; }
    }
}
