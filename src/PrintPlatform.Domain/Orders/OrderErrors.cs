using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Orders;

/// <summary>Centralised business errors for the Orders + Quoting module.</summary>
public static class OrderErrors
{
    // ModelFile
    public static readonly Error ModelFileNotFound =
        Error.NotFound("ModelFile.NotFound", "The requested model file was not found.");

    public static Error UnsupportedFormat(string ext) =>
        Error.Validation("ModelFile.UnsupportedFormat", $"File format '{ext}' is not supported. Allowed: STL, 3MF, OBJ.");

    public static readonly Error FileTooLarge =
        Error.Validation("ModelFile.TooLarge", "The uploaded file exceeds the 50 MB limit.");

    public static readonly Error EmptyFile =
        Error.Validation("ModelFile.Empty", "The uploaded file is empty.");

    public static readonly Error ModelNotAnalysed =
        Error.Conflict("ModelFile.NotAnalysed", "The model has not completed geometry analysis yet.");

    // QuoteRequest
    public static readonly Error QuoteRequestNotFound =
        Error.NotFound("QuoteRequest.NotFound", "The requested quote request was not found.");

    public static readonly Error InvalidQuantity =
        Error.Validation("QuoteRequest.InvalidQuantity", "Quantity must be at least 1.");

    public static readonly Error InvalidInfill =
        Error.Validation("QuoteRequest.InvalidInfill", "Infill percent must be between 0 and 100.");

    public static readonly Error InvalidLayerHeight =
        Error.Validation("QuoteRequest.InvalidLayerHeight", "Layer height must be greater than 0.");

    // Quote
    public static readonly Error QuoteNotFound =
        Error.NotFound("Quote.NotFound", "The requested quote was not found.");

    public static readonly Error QuoteAlreadyExists =
        Error.Conflict("Quote.AlreadyExists", "A quote already exists for this quote request.");

    public static readonly Error QuoteNotSent =
        Error.Conflict("Quote.NotSent", "The quote has not been confirmed/sent and cannot be accepted.");

    public static readonly Error QuoteExpired =
        Error.Conflict("Quote.Expired", "The quote has expired and can no longer be accepted.");

    public static readonly Error QuoteAlreadyAccepted =
        Error.Conflict("Quote.AlreadyAccepted", "The quote has already been accepted.");

    // Order
    public static readonly Error OrderNotFound =
        Error.NotFound("Order.NotFound", "The requested order was not found.");

    public static readonly Error OrderNotPendingPayment =
        Error.Conflict("Order.NotPendingPayment", "The order is not awaiting payment.");

    public static Error InvalidStatusTransition(OrderStatus from, OrderStatus to) =>
        Error.Conflict("Order.InvalidTransition", $"Cannot transition order from {from} to {to}.");

    public static readonly Error OrderNotCancellable =
        Error.Conflict("Order.NotCancellable", "The order can no longer be cancelled at its current stage.");

    // Payment
    public static readonly Error PaymentInitiationFailed =
        Error.Failure("Payment.InitiationFailed", "Failed to initiate payment with the gateway.");

    public static readonly Error InvalidWebhookSignature =
        Error.Unauthorized("Payment.InvalidSignature", "The payment webhook HMAC signature is invalid.");
}
