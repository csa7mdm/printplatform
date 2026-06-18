using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Orders.Commands;

/// <summary>
/// Customer accepts a sent quote. Validates expiry/state, marks the quote + request
/// Accepted, and creates a new <see cref="Order"/> in <see cref="OrderStatus.PendingPayment"/>.
/// Loyalty redemption (if any) is passed in pre-resolved as points + EGP discount.
/// </summary>
public sealed record AcceptQuoteCommand(
    Guid QuoteId,
    Guid CustomerUserId,
    PaymentMethod PaymentMethod,
    ShippingAddress ShippingAddress,
    decimal DiscountEgp = 0m,
    long LoyaltyPointsUsed = 0,
    decimal LoyaltyDiscountEgp = 0m) : IRequest<Result<OrderDetailsDto>>;

/// <summary>Value object captured as a JSON snapshot on the order.</summary>
public sealed record ShippingAddress(
    string RecipientName,
    string Phone,
    string Street,
    string City,
    string Governorate,
    string Country = "Egypt",
    string? PostalCode = null,
    string? Notes = null);

public sealed class AcceptQuoteHandler : IRequestHandler<AcceptQuoteCommand, Result<OrderDetailsDto>>
{
    private readonly IAppDbContext _db;
    private readonly TimeProvider _clock;

    public AcceptQuoteHandler(IAppDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<OrderDetailsDto>> Handle(
        AcceptQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await _db.Quotes
            .FirstOrDefaultAsync(q => q.Id == request.QuoteId, cancellationToken);
        if (quote is null)
            return Result.Failure<OrderDetailsDto>(OrderErrors.QuoteNotFound);

        var quoteRequest = await _db.QuoteRequests
            .FirstOrDefaultAsync(r => r.Id == quote.QuoteRequestId, cancellationToken);
        if (quoteRequest is null)
            return Result.Failure<OrderDetailsDto>(OrderErrors.QuoteRequestNotFound);

        if (quoteRequest.CustomerUserId != request.CustomerUserId)
            return Result.Failure<OrderDetailsDto>(OrderErrors.QuoteNotFound);

        var model = await _db.ModelFiles
            .FirstOrDefaultAsync(m => m.Id == quoteRequest.ModelFileId, cancellationToken);
        if (model is null)
            return Result.Failure<OrderDetailsDto>(OrderErrors.ModelFileNotFound);

        var accept = quote.Accept(_clock.GetUtcNow());
        if (accept.IsFailure)
            return Result.Failure<OrderDetailsDto>(accept.Error);

        quoteRequest.MarkAccepted();

        var addressJson = JsonSerializer.Serialize(request.ShippingAddress);

        var order = Order.CreateFromAcceptedQuote(
            customerUserId: request.CustomerUserId,
            quoteId: quote.Id,
            quantity: quoteRequest.Quantity,
            unitPriceEgp: quote.CustomerPriceEgp + quote.DesignServicePriceEgp,
            modelFileStorageKey: model.StorageKey,
            paymentMethod: request.PaymentMethod,
            shippingAddressSnapshot: addressJson,
            deliveryFeeEgp: quote.DeliveryEstimateEgp,
            discountEgp: request.DiscountEgp,
            loyaltyPointsUsed: request.LoyaltyPointsUsed,
            loyaltyDiscountEgp: request.LoyaltyDiscountEgp);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        return ModelFileMapper.ToDetails(order);
    }
}
