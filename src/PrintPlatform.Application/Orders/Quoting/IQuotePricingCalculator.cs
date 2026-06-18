using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Application.Orders.Quoting;

/// <summary>The computed cost / price breakdown for a quote.</summary>
public sealed record QuotePricingResult(
    decimal EstimatedWeightGrams,
    decimal EstimatedPrintHours,
    decimal MaterialCostEgp,
    decimal DepreciationCostEgp,
    decimal LaborCostEgp,
    decimal PackagingCostEgp,
    decimal FailureAllowanceEgp,
    decimal PlatformMarginEgp,
    decimal DesignServicePriceEgp,
    decimal DeliveryEstimateEgp,
    DateTimeOffset EstimatedDeliveryDate,
    DateTimeOffset ValidUntil);

/// <summary>
/// Computes a default quote pricing breakdown from the analysed model + request.
/// Operators may override the resulting figures before sending the quote.
/// </summary>
public interface IQuotePricingCalculator
{
    QuotePricingResult Calculate(ModelFile model, QuoteRequest request);
}
