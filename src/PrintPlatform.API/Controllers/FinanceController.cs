using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Application.Finance;
using PrintPlatform.Domain.Finance;

namespace PrintPlatform.API.Controllers;

[Authorize]
[Route("api/finance")]
[Produces("application/json")]
public sealed class FinanceController : ApiControllerBase
{
    [HttpGet("my-earnings")]
    [ProducesResponseType(typeof(OwnerEarningsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyEarnings(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOwnerEarningsQuery(CurrentUserId), ct);
        return ToActionResult(result);
    }

    [HttpGet("my-earnings/breakdown")]
    [ProducesResponseType(typeof(OwnerEarningsBreakdownDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyEarningsBreakdown(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOwnerEarningsBreakdownQuery(CurrentUserId, from, to), ct);
        return ToActionResult(result);
    }

    [HttpGet("payouts")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IReadOnlyList<PayoutDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingPayouts(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPendingPayoutsQuery(), ct);
        return ToActionResult(result);
    }

    [HttpPost("payouts/{id:guid}/mark-sent")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(PayoutDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkPayoutSent(
        Guid id,
        [FromBody] MarkPayoutSentRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new MarkPayoutSentCommand(id, request.ExternalReference, CurrentUserId),
            ct);
        return ToActionResult(result);
    }

    [HttpGet("platform/revenue")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(PlatformRevenueDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlatformRevenue(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPlatformRevenueQuery(from, to), ct);
        return ToActionResult(result);
    }

    [HttpGet("ledger")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IReadOnlyList<LedgerEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLedgerAudit(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] Guid? relatedOrderId,
        [FromQuery] Guid? printerOwnerUserId,
        [FromQuery] Guid? customerUserId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetLedgerAuditQuery(from, to, relatedOrderId, printerOwnerUserId, customerUserId),
            ct);
        return ToActionResult(result);
    }

    [HttpPost("payments/customer")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IReadOnlyList<LedgerEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordCustomerPayment(
        [FromBody] RecordCustomerPaymentRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RecordCustomerPaymentCommand(
                request.OrderId,
                request.AmountEgp,
                request.ExternalReference,
                CurrentUserId),
            ct);
        return ToActionResult(result);
    }

    [HttpPost("refunds")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(LedgerEntryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordRefund(
        [FromBody] RecordRefundRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RecordRefundCommand(
                request.OrderId,
                request.AmountEgp,
                request.ExternalReference,
                CurrentUserId),
            ct);
        return ToActionResult(result);
    }

    [HttpPost("payouts/initiate-weekly")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<PayoutDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> InitiateWeeklyPayouts(
        [FromBody] InitiateWeeklyPayoutsRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new InitiateWeeklyPayoutsCommand(request.PeriodStart, request.PeriodEnd, request.PaymentMethod),
            ct);
        return ToActionResult(result);
    }
}

public sealed record MarkPayoutSentRequest(string ExternalReference);

public sealed record RecordCustomerPaymentRequest(
    Guid OrderId,
    decimal AmountEgp,
    string? ExternalReference);

public sealed record RecordRefundRequest(
    Guid OrderId,
    decimal AmountEgp,
    string? ExternalReference);

public sealed record InitiateWeeklyPayoutsRequest(
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    PayoutPaymentMethod PaymentMethod = PayoutPaymentMethod.InstaPay);
