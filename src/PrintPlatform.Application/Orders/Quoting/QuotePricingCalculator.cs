using Microsoft.Extensions.Options;
using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Application.Orders.Quoting;

/// <summary>
/// Default quoting engine. Derives weight/print-time from the analysed model (falling
/// back to safe defaults if analysis figures are absent) and the requested quality
/// preset, then layers the configured cost components, failure allowance and margin.
/// </summary>
public sealed class QuotePricingCalculator : IQuotePricingCalculator
{
    private readonly QuotePricingOptions _options;
    private readonly TimeProvider _clock;

    public QuotePricingCalculator(IOptions<QuotePricingOptions> options, TimeProvider clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public QuotePricingResult Calculate(ModelFile model, QuoteRequest request)
    {
        var o = _options;
        var qty = request.Quantity;

        // Base per-unit figures from analysis (defensive defaults if not yet analysed).
        var unitWeight = model.EstimatedWeightGrams ?? 0m;
        var unitHours = model.EstimatedPrintHours ?? 0m;

        // Quality preset scales print time (finer layers => slower) and infill scales weight.
        var qualityTimeMultiplier = request.QualityPreset switch
        {
            QualityPreset.Draft => 0.7m,
            QualityPreset.Standard => 1.0m,
            QualityPreset.Fine => 1.6m,
            QualityPreset.Ultra => 2.4m,
            _ => 1.0m,
        };
        // Infill at 20% is the analysis baseline; deviation scales weight proportionally.
        var infillMultiplier = request.InfillPercent <= 0 ? 1.0m : request.InfillPercent / 20.0m;

        var totalWeight = unitWeight * infillMultiplier * qty;
        var totalHours = unitHours * qualityTimeMultiplier * qty;

        var materialCost = decimal.Round(totalWeight * o.MaterialCostPerGramEgp, 2);
        var depreciationCost = decimal.Round(totalHours * o.DepreciationCostPerHourEgp, 2);
        var laborCost = decimal.Round(totalHours * o.LaborCostPerHourEgp, 2);
        var packagingCost = o.PackagingCostEgp;

        var preAllowance = materialCost + depreciationCost + laborCost + packagingCost;
        var failureAllowance = decimal.Round(preAllowance * o.FailureAllowanceRate, 2);

        var fulfillmentCost = preAllowance + failureAllowance;
        var margin = decimal.Round(fulfillmentCost * o.PlatformMarginRate, 2);

        var designService = request.IncludeDesignService ? o.DesignServicePriceEgp : 0m;
        var delivery = o.DefaultDeliveryEstimateEgp;

        var now = _clock.GetUtcNow();
        var printDays = (int)Math.Ceiling((double)totalHours / 8.0); // ~8 print hours / working day
        var estimatedDelivery = now.AddDays(o.BaseLeadTimeDays + printDays);
        var validUntil = now.AddDays(o.QuoteValidityDays);

        return new QuotePricingResult(
            totalWeight,
            totalHours,
            materialCost,
            depreciationCost,
            laborCost,
            packagingCost,
            failureAllowance,
            margin,
            designService,
            delivery,
            estimatedDelivery,
            validUntil);
    }
}
