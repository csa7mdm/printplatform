namespace PrintPlatform.Application.Orders.Quoting;

/// <summary>
/// Configurable pricing parameters for the quoting engine (bound from the
/// "Quoting" config section). All monetary values are in EGP.
/// </summary>
public sealed class QuotePricingOptions
{
    public const string SectionName = "Quoting";

    /// <summary>Material cost per gram (EGP). Default approximates PLA in the Egyptian market.</summary>
    public decimal MaterialCostPerGramEgp { get; set; } = 1.20m;

    /// <summary>Printer depreciation + electricity cost charged per print hour (EGP).</summary>
    public decimal DepreciationCostPerHourEgp { get; set; } = 8.00m;

    /// <summary>Operator labour cost per print hour (EGP) for setup / monitoring / post-processing.</summary>
    public decimal LaborCostPerHourEgp { get; set; } = 15.00m;

    /// <summary>Flat packaging cost per order (EGP).</summary>
    public decimal PackagingCostEgp { get; set; } = 20.00m;

    /// <summary>Fraction of fulfilment cost reserved to cover failed prints (e.g. 0.10 = 10%).</summary>
    public decimal FailureAllowanceRate { get; set; } = 0.10m;

    /// <summary>Platform margin as a fraction of total fulfilment cost (e.g. 0.35 = 35%).</summary>
    public decimal PlatformMarginRate { get; set; } = 0.35m;

    /// <summary>Flat design-service price (EGP) added when the customer requests it.</summary>
    public decimal DesignServicePriceEgp { get; set; } = 250.00m;

    /// <summary>Default delivery fee estimate (EGP).</summary>
    public decimal DefaultDeliveryEstimateEgp { get; set; } = 60.00m;

    /// <summary>Number of days a quote remains valid after being produced.</summary>
    public int QuoteValidityDays { get; set; } = 14;

    /// <summary>Lead-time in days added on top of estimated print time to set delivery date.</summary>
    public int BaseLeadTimeDays { get; set; } = 3;
}
