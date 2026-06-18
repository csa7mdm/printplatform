using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Application.Orders;

/// <summary>Hand-written projections for Orders aggregates (avoids reflection on hot paths).</summary>
internal static class ModelFileMapper
{
    public static ModelFileDto ToDto(ModelFile m) => new(
        m.Id, m.CustomerUserId, m.OriginalFileName, m.FileSizeBytes, m.FileFormat, m.Status,
        m.EstimatedVolumeCC, m.BoundingBoxX, m.BoundingBoxY, m.BoundingBoxZ,
        m.EstimatedWeightGrams, m.EstimatedPrintHours, m.AnalysisNotes, m.CreatedAt);

    public static QuoteDto ToDto(Quote q) => new(
        q.Id, q.QuoteRequestId, q.EstimatedWeightGrams, q.EstimatedPrintHours,
        q.CustomerPriceEgp, q.DesignServicePriceEgp, q.DeliveryEstimateEgp, q.TotalPriceEgp,
        q.EstimatedDeliveryDate, q.ValidUntil, q.OperatorNotes, q.Status);

    public static OrderDetailsDto ToDetails(Order o) => new(
        o.Id, o.CustomerUserId, o.Status, o.PaymentStatus, o.PaymentMethod, o.PaymobOrderId,
        o.ShippingAddressSnapshot, o.SubtotalEgp, o.DeliveryFeeEgp, o.DiscountEgp,
        o.LoyaltyDiscountEgp, o.LoyaltyPointsUsed, o.TotalEgp,
        o.Items.Select(i => new OrderItemDto(i.Id, i.QuoteId, i.Quantity, i.UnitPriceEgp, i.TotalEgp)).ToList(),
        o.CreatedAt);

    public static OrderSummaryDto ToSummary(Order o) => new(
        o.Id, o.Status, o.PaymentStatus, o.PaymentMethod, o.TotalEgp, o.CreatedAt);
}
