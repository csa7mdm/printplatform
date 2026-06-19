using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Application.Orders;
using PrintPlatform.Application.Orders.Commands;
using PrintPlatform.Application.Orders.Queries;

namespace PrintPlatform.API.Controllers;

[Authorize]
[Route("api/orders")]
[Produces("application/json")]
public sealed class OrdersController : ApiControllerBase
{
    /// <summary>Lists orders for the authenticated customer.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMyOrdersQuery(CurrentUserId), ct);
        return Ok(result);
    }

    /// <summary>Returns details for a customer order.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOrderDetailsQuery(id, CurrentUserId), ct);
        return ToActionResult(result);
    }

    /// <summary>Initiates Paymob payment for a pending-payment order.</summary>
    [HttpPost("{id:guid}/payment/initiate")]
    [ProducesResponseType(typeof(InitiatePaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InitiatePayment(
        Guid id,
        [FromBody] InitiatePaymentRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new InitiatePaymentCommand(
                id,
                CurrentUserId,
                request.CustomerFirstName,
                request.CustomerLastName,
                request.CustomerEmail,
                request.CustomerPhone),
            ct);

        return ToActionResult(result);
    }

    /// <summary>Cancels a customer order if it has not shipped.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelOrderRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new CancelOrderCommand(id, CurrentUserId, request.Reason), ct);
        return ToActionResult(result);
    }
}

public sealed record InitiatePaymentRequest(
    string CustomerFirstName,
    string CustomerLastName,
    string CustomerEmail,
    string CustomerPhone);

public sealed record CancelOrderRequest(string Reason);
