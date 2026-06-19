using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Application.Orders;
using PrintPlatform.Application.Orders.Commands;
using PrintPlatform.Application.Orders.Queries;
using PrintPlatform.Domain.Orders;

namespace PrintPlatform.API.Controllers;

[Authorize]
[Route("api/quotes")]
[Produces("application/json")]
public sealed class QuotesController : ApiControllerBase
{
    /// <summary>Creates a quote request for an analysed model.</summary>
    [HttpPost("request")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestQuote(
        [FromBody] CreateQuoteRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateQuoteRequestCommand(
                CurrentUserId,
                request.ModelFileId,
                request.MaterialOptionId,
                request.QualityPreset,
                request.LayerHeightMm,
                request.InfillPercent,
                request.Quantity,
                request.IncludeDesignService),
            ct);

        return ToActionResult(result);
    }

    /// <summary>Lists quotes for the authenticated customer.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<QuoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMyQuotesQuery(CurrentUserId), ct);
        return ToActionResult(result);
    }

    /// <summary>Returns a single quote for the authenticated customer.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetQuoteDetailsQuery(id, CurrentUserId), ct);
        return ToActionResult(result);
    }

    /// <summary>Confirms operator pricing for a pending quote request.</summary>
    [HttpPut("{id:guid}/confirm")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(
        Guid id,
        [FromBody] ConfirmQuoteRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ConfirmQuoteCommand(
                id,
                request.OverrideCustomerPriceEgp,
                request.OverrideDesignServicePriceEgp,
                request.OverrideDeliveryEstimateEgp,
                request.OverrideValidUntil,
                request.OperatorNotes),
            ct);

        return ToActionResult(result);
    }

    /// <summary>Accepts a sent quote and creates a pending-payment order.</summary>
    [HttpPost("{id:guid}/accept")]
    [ProducesResponseType(typeof(OrderDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Accept(
        Guid id,
        [FromBody] AcceptQuoteRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AcceptQuoteCommand(
                id,
                CurrentUserId,
                request.PaymentMethod,
                request.ShippingAddress,
                request.DiscountEgp,
                request.LoyaltyPointsUsed,
                request.LoyaltyDiscountEgp),
            ct);

        return ToActionResult(result);
    }
}

public sealed record CreateQuoteRequest(
    Guid ModelFileId,
    Guid MaterialOptionId,
    QualityPreset QualityPreset,
    decimal LayerHeightMm,
    int InfillPercent,
    int Quantity,
    bool IncludeDesignService);

public sealed record ConfirmQuoteRequest(
    decimal? OverrideCustomerPriceEgp,
    decimal? OverrideDesignServicePriceEgp,
    decimal? OverrideDeliveryEstimateEgp,
    DateTimeOffset? OverrideValidUntil,
    string? OperatorNotes);

public sealed record AcceptQuoteRequest(
    PaymentMethod PaymentMethod,
    ShippingAddress ShippingAddress,
    decimal DiscountEgp = 0m,
    long LoyaltyPointsUsed = 0,
    decimal LoyaltyDiscountEgp = 0m);
