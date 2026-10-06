using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Application.Orders.Commands;

namespace PrintPlatform.API.Controllers;

[Route("api/payments")]
[Produces("application/json")]
public sealed class PaymentsController : ApiControllerBase
{
    private readonly IPaymentGateway _gateway;

    public PaymentsController(IPaymentGateway gateway)
    {
        _gateway = gateway;
    }

    /// <summary>Processes a Paymob payment webhook. HMAC is verified before any state change.</summary>
    [AllowAnonymous]
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        Request.EnableBuffering();
        string raw;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            raw = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        using var document = JsonDocument.Parse(raw);
        var root = document.RootElement;

        // Security: reject forged callbacks before touching any state.
        if (!VerifyHmac(root))
            return Unauthorized("Invalid Paymob HMAC signature.");

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

    private bool VerifyHmac(JsonElement root)
    {
        var hmac = TryGet(root, "hmac", out var h) ? TryGetString(h) : null;
        if (string.IsNullOrWhiteSpace(hmac)) return false;

        var obj = TryGet(root, "obj", out var o) ? o : root;
        var payload = FlattenForHmac(obj);
        return _gateway.ValidateWebhookSignature(payload, hmac!);
    }

    // Paymob's HMAC-SHA512 concatenates these fields in this exact order.
    private static readonly string[] HmacKeys =
    [
        "amount_cents","created_at","currency","error_occured","has_parent_transaction","id",
        "integration_id","is_3d_secure","is_auth","is_capture","is_refunded","is_standalone_payment",
        "is_voided","order.id","owner","pending","source_data.pan","source_data.sub_type",
        "source_data.type","success",
    ];

    private static Dictionary<string, string> FlattenForHmac(JsonElement obj)
    {
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var key in HmacKeys)
            dict[key] = ResolvePath(obj, key);
        return dict;
    }

    private static string ResolvePath(JsonElement obj, string path)
    {
        var current = obj;
        foreach (var part in path.Split('.'))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(part, out current))
                return string.Empty;
        }
        return ScalarToString(current);
    }

    private static string ScalarToString(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        _ => string.Empty,
    };

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