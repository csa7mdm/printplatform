namespace PrintPlatform.Application.Abstractions;

/// <summary>Request to initiate a Paymob payment for an order.</summary>
public sealed record InitiatePaymentRequest(
    Guid OrderId,
    decimal AmountEgp,
    string CustomerFirstName,
    string CustomerLastName,
    string CustomerEmail,
    string CustomerPhone,
    string ShippingStreet,
    string ShippingCity,
    string ShippingCountry);

/// <summary>Result of a successful payment initiation.</summary>
/// <param name="PaymobOrderId">Paymob's internal order id (used to correlate webhooks).</param>
/// <param name="PaymentKey">The payment key (token) used to open the iframe.</param>
/// <param name="IframeUrl">The fully-formed iframe URL the client should redirect to.</param>
public sealed record InitiatePaymentResult(
    string PaymobOrderId,
    string PaymentKey,
    string IframeUrl);

/// <summary>Abstraction over a payment gateway (Paymob). Concrete impl lives in Infrastructure.</summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Runs the Paymob v2 flow: auth token → order registration → payment key →
    /// iframe URL. Throws on hard gateway failures; callers wrap in a Result.
    /// </summary>
    Task<InitiatePaymentResult> InitiatePaymentAsync(
        InitiatePaymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a webhook callback's HMAC signature against the configured key.
    /// </summary>
    bool ValidateWebhookSignature(IReadOnlyDictionary<string, string> payloadObj, string hmac);
}
