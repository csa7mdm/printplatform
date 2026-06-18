using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Orders;

/// <summary>
/// An operator-produced price quote tied 1:1 to a <see cref="QuoteRequest"/>.
/// Carries the full cost breakdown (fulfilment cost components + margin) used to
/// arrive at the customer-facing price.
/// </summary>
public sealed class Quote : BaseAggregateRoot<Guid>
{
    private Quote() { } // EF

    private Quote(Guid id, Guid quoteRequestId)
    {
        Id = id;
        QuoteRequestId = quoteRequestId;
        Status = QuoteStatus.Draft;
    }

    public Guid QuoteRequestId { get; private set; }

    public decimal EstimatedWeightGrams { get; private set; }
    public decimal EstimatedPrintHours { get; private set; }

    // --- Fulfilment cost breakdown (internal) -------------------------------
    public decimal MaterialCostEgp { get; private set; }
    public decimal DepreciationCostEgp { get; private set; }
    public decimal LaborCostEgp { get; private set; }
    public decimal PackagingCostEgp { get; private set; }
    public decimal FailureAllowanceEgp { get; private set; }
    public decimal TotalFulfillmentCostEgp { get; private set; }

    // --- Customer-facing pricing --------------------------------------------
    public decimal PlatformMarginEgp { get; private set; }
    public decimal CustomerPriceEgp { get; private set; }
    public decimal DesignServicePriceEgp { get; private set; }
    public decimal DeliveryEstimateEgp { get; private set; }
    public decimal TotalPriceEgp { get; private set; }

    public DateTimeOffset? EstimatedDeliveryDate { get; private set; }
    public DateTimeOffset ValidUntil { get; private set; }
    public string? OperatorNotes { get; private set; }
    public QuoteStatus Status { get; private set; }

    /// <summary>Creates a draft quote for a quote request.</summary>
    public static Quote CreateDraft(Guid quoteRequestId) =>
        new(Guid.NewGuid(), quoteRequestId);

    /// <summary>
    /// Populates the full cost/price breakdown. <see cref="TotalFulfillmentCostEgp"/>
    /// and <see cref="TotalPriceEgp"/> are recomputed from the components.
    /// </summary>
    public void SetPricing(
        decimal estimatedWeightGrams,
        decimal estimatedPrintHours,
        decimal materialCostEgp,
        decimal depreciationCostEgp,
        decimal laborCostEgp,
        decimal packagingCostEgp,
        decimal failureAllowanceEgp,
        decimal platformMarginEgp,
        decimal designServicePriceEgp,
        decimal deliveryEstimateEgp,
        DateTimeOffset estimatedDeliveryDate,
        DateTimeOffset validUntil,
        string? operatorNotes)
    {
        EstimatedWeightGrams = estimatedWeightGrams;
        EstimatedPrintHours = estimatedPrintHours;

        MaterialCostEgp = materialCostEgp;
        DepreciationCostEgp = depreciationCostEgp;
        LaborCostEgp = laborCostEgp;
        PackagingCostEgp = packagingCostEgp;
        FailureAllowanceEgp = failureAllowanceEgp;
        TotalFulfillmentCostEgp =
            materialCostEgp + depreciationCostEgp + laborCostEgp + packagingCostEgp + failureAllowanceEgp;

        PlatformMarginEgp = platformMarginEgp;
        CustomerPriceEgp = TotalFulfillmentCostEgp + platformMarginEgp;
        DesignServicePriceEgp = designServicePriceEgp;
        DeliveryEstimateEgp = deliveryEstimateEgp;
        TotalPriceEgp = CustomerPriceEgp + designServicePriceEgp + deliveryEstimateEgp;

        EstimatedDeliveryDate = estimatedDeliveryDate;
        ValidUntil = validUntil;
        OperatorNotes = operatorNotes;
        Touch();
    }

    /// <summary>Operator confirms and sends the quote to the customer.</summary>
    public void Send()
    {
        Status = QuoteStatus.Sent;
        Touch();
    }

    /// <summary>Customer accepts a sent, non-expired quote. Returns a failure Result otherwise.</summary>
    public Result Accept(DateTimeOffset now)
    {
        if (Status == QuoteStatus.Accepted) return Result.Failure(OrderErrors.QuoteAlreadyAccepted);
        if (Status != QuoteStatus.Sent) return Result.Failure(OrderErrors.QuoteNotSent);
        if (now > ValidUntil)
        {
            Status = QuoteStatus.Expired;
            Touch();
            return Result.Failure(OrderErrors.QuoteExpired);
        }

        Status = QuoteStatus.Accepted;
        Touch();
        return Result.Success();
    }

    public bool IsExpired(DateTimeOffset now) => now > ValidUntil;
}
