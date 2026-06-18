using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Application.Orders;

public sealed record ModelFileDto(
    Guid Id,
    Guid CustomerUserId,
    string OriginalFileName,
    long FileSizeBytes,
    FileFormat FileFormat,
    ModelFileStatus Status,
    decimal? EstimatedVolumeCC,
    decimal? BoundingBoxX,
    decimal? BoundingBoxY,
    decimal? BoundingBoxZ,
    decimal? EstimatedWeightGrams,
    decimal? EstimatedPrintHours,
    string? AnalysisNotes,
    DateTimeOffset CreatedAt);

public sealed record QuoteDto(
    Guid Id,
    Guid QuoteRequestId,
    decimal EstimatedWeightGrams,
    decimal EstimatedPrintHours,
    decimal CustomerPriceEgp,
    decimal DesignServicePriceEgp,
    decimal DeliveryEstimateEgp,
    decimal TotalPriceEgp,
    DateTimeOffset? EstimatedDeliveryDate,
    DateTimeOffset ValidUntil,
    string? OperatorNotes,
    QuoteStatus Status);

public sealed record PendingQuoteDto(
    Guid QuoteRequestId,
    Guid ModelFileId,
    Guid CustomerUserId,
    Guid MaterialOptionId,
    QualityPreset QualityPreset,
    int Quantity,
    bool IncludeDesignService,
    QuoteRequestStatus Status,
    DateTimeOffset CreatedAt);

public sealed record OrderItemDto(
    Guid Id,
    Guid QuoteId,
    int Quantity,
    decimal UnitPriceEgp,
    decimal TotalEgp);

public sealed record OrderSummaryDto(
    Guid Id,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    PaymentMethod PaymentMethod,
    decimal TotalEgp,
    DateTimeOffset CreatedAt);

public sealed record OrderDetailsDto(
    Guid Id,
    Guid CustomerUserId,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    PaymentMethod PaymentMethod,
    string? PaymobOrderId,
    string ShippingAddressSnapshot,
    decimal SubtotalEgp,
    decimal DeliveryFeeEgp,
    decimal DiscountEgp,
    decimal LoyaltyDiscountEgp,
    long LoyaltyPointsUsed,
    decimal TotalEgp,
    IReadOnlyList<OrderItemDto> Items,
    DateTimeOffset CreatedAt);

public sealed record InitiatePaymentResponse(
    string PaymobOrderId,
    string PaymentKey,
    string IframeUrl);
