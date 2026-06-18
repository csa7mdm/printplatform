using MediatR;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Application.Orders.Quoting;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Orders.Commands;

/// <summary>
/// Operator action: produces (or re-prices) the final <see cref="Quote"/> for a
/// pending <see cref="QuoteRequest"/> and sends it to the customer. The default
/// pricing engine computes the breakdown; operators may override any figure via the
/// optional override fields. Marks the request <see cref="QuoteRequestStatus.Quoted"/>.
/// </summary>
public sealed record ConfirmQuoteCommand(
    Guid QuoteRequestId,
    decimal? OverrideCustomerPriceEgp = null,
    decimal? OverrideDesignServicePriceEgp = null,
    decimal? OverrideDeliveryEstimateEgp = null,
    DateTimeOffset? OverrideValidUntil = null,
    string? OperatorNotes = null) : IRequest<Result<QuoteDto>>;

public sealed class ConfirmQuoteHandler : IRequestHandler<ConfirmQuoteCommand, Result<QuoteDto>>
{
    private readonly IAppDbContext _db;
    private readonly IQuotePricingCalculator _calculator;

    public ConfirmQuoteHandler(IAppDbContext db, IQuotePricingCalculator calculator)
    {
        _db = db;
        _calculator = calculator;
    }

    public async Task<Result<QuoteDto>> Handle(
        ConfirmQuoteCommand request, CancellationToken cancellationToken)
    {
        var quoteRequest = await _db.QuoteRequests
            .FirstOrDefaultAsync(q => q.Id == request.QuoteRequestId, cancellationToken);
        if (quoteRequest is null)
            return Result.Failure<QuoteDto>(OrderErrors.QuoteRequestNotFound);

        var model = await _db.ModelFiles
            .FirstOrDefaultAsync(m => m.Id == quoteRequest.ModelFileId, cancellationToken);
        if (model is null)
            return Result.Failure<QuoteDto>(OrderErrors.ModelFileNotFound);

        if (model.Status != ModelFileStatus.Approved)
            return Result.Failure<QuoteDto>(OrderErrors.ModelNotAnalysed);

        var pricing = _calculator.Calculate(model, quoteRequest);

        // Reuse an existing quote for this request if present (re-pricing), else create.
        var quote = await _db.Quotes
            .FirstOrDefaultAsync(q => q.QuoteRequestId == quoteRequest.Id, cancellationToken);
        var isNew = quote is null;
        quote ??= Quote.CreateDraft(quoteRequest.Id);

        // Operator overrides: customer price override is applied by adjusting the margin so
        // the recomputed CustomerPrice matches the requested figure.
        var customerPrice = request.OverrideCustomerPriceEgp;
        var fulfillment = pricing.MaterialCostEgp + pricing.DepreciationCostEgp
                          + pricing.LaborCostEgp + pricing.PackagingCostEgp + pricing.FailureAllowanceEgp;
        var margin = customerPrice.HasValue
            ? customerPrice.Value - fulfillment
            : pricing.PlatformMarginEgp;

        quote.SetPricing(
            estimatedWeightGrams: pricing.EstimatedWeightGrams,
            estimatedPrintHours: pricing.EstimatedPrintHours,
            materialCostEgp: pricing.MaterialCostEgp,
            depreciationCostEgp: pricing.DepreciationCostEgp,
            laborCostEgp: pricing.LaborCostEgp,
            packagingCostEgp: pricing.PackagingCostEgp,
            failureAllowanceEgp: pricing.FailureAllowanceEgp,
            platformMarginEgp: margin,
            designServicePriceEgp: request.OverrideDesignServicePriceEgp ?? pricing.DesignServicePriceEgp,
            deliveryEstimateEgp: request.OverrideDeliveryEstimateEgp ?? pricing.DeliveryEstimateEgp,
            estimatedDeliveryDate: pricing.EstimatedDeliveryDate,
            validUntil: request.OverrideValidUntil ?? pricing.ValidUntil,
            operatorNotes: request.OperatorNotes);

        quote.Send();
        quoteRequest.MarkQuoted();

        if (isNew)
            _db.Quotes.Add(quote);

        await _db.SaveChangesAsync(cancellationToken);

        return ModelFileMapper.ToDto(quote);
    }
}
