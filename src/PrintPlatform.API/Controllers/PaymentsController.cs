using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Application.Orders.Commands;

namespace PrintPlatform.API.Controllers;

[Route("api/payments")]
[Produces("application/json")]
public sealed class PaymentsController : ApiControllerBase
{
    /// <summary>Processes a Paymob payment webhook.</summary>
    [AllowAnonymous]
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        using var document = await JsonDocument.ParseAsync(Request.Body, cancellationToken: ct);
        var root = document.RootElement;
        var obj = TryGet(root, "obj", out var objElement) ? objElement : root;

        var paymobOrderId = ResolvePaymobOrderId(obj);
        var success = ResolveBool(obj, "success")
                      ?? ResolveBool(obj, "is_success")
                      ?? ResolveBool(obj, "is_auth")
                      ?? false;
        var amountCents = ResolveDecimal(obj, "amount_cents") ?? 0m;

        if (string.IsNullOrWhiteSpace(paymobOrderId))
            return BadRequest("Paymob order id is required.");

        var result = await Mediator.Send(
            new ConfirmPaymentCommand(paymobOrderId, success, amountCents),
            ct);

        return ToActionResult(result);
    }

    private static string? ResolvePaymobOrderId(JsonElement obj)
    {
        if (TryGet(obj, "order", out var order))
        {
            if (order.ValueKind == JsonValueKind.Object
                && TryGet(order, "id", out var nestedOrderId)
                && TryGetString(nestedOrderId) is { } nestedId)
                return nestedId;

            if (TryGetString(order) is { } directOrder)
                return directOrder;
        }

        if (TryGet(obj, "order_id", out var orderIdElement)
            && TryGetString(orderIdElement) is { } orderId)
            return orderId;

        if (TryGet(obj, "paymob_order_id", out var paymobOrderIdElement)
            && TryGetString(paymobOrderIdElement) is { } paymobOrderId)
            return paymobOrderId;

        return null;
    }

    private static bool? ResolveBool(JsonElement obj, string propertyName)
    {
        if (!TryGet(obj, propertyName, out var value))
            return null;

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(value.GetString(), out var parsed) => parsed,
            JsonValueKind.Number when value.TryGetInt32(out var number) => number != 0,
            _ => null
        };
    }

    private static decimal? ResolveDecimal(JsonElement obj, string propertyName)
    {
        if (!TryGet(obj, propertyName, out var value))
            return null;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
            return number;

        if (value.ValueKind == JsonValueKind.String
            && decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        return null;
    }

    private static string? TryGetString(JsonElement value)
        => value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null
        };

    private static bool TryGet(JsonElement obj, string propertyName, out JsonElement value)
    {
        if (obj.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        return obj.TryGetProperty(propertyName, out value);
    }
}
