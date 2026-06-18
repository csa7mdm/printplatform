using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Orders;

/// <summary>
/// A customer's request for a price quote on a specific model + material + quality
/// configuration. Reviewed by an operator who produces a <see cref="Quote"/>.
/// </summary>
public sealed class QuoteRequest : BaseAggregateRoot<Guid>
{
    private QuoteRequest() { } // EF

    private QuoteRequest(
        Guid id,
        Guid modelFileId,
        Guid customerUserId,
        Guid materialOptionId,
        QualityPreset qualityPreset,
        decimal layerHeightMm,
        int infillPercent,
        int quantity,
        bool includeDesignService)
    {
        Id = id;
        ModelFileId = modelFileId;
        CustomerUserId = customerUserId;
        MaterialOptionId = materialOptionId;
        QualityPreset = qualityPreset;
        LayerHeightMm = layerHeightMm;
        InfillPercent = infillPercent;
        Quantity = quantity;
        IncludeDesignService = includeDesignService;
        Status = QuoteRequestStatus.PendingReview;
    }

    public Guid ModelFileId { get; private set; }
    public Guid CustomerUserId { get; private set; }
    public Guid MaterialOptionId { get; private set; }
    public QualityPreset QualityPreset { get; private set; }
    public decimal LayerHeightMm { get; private set; }
    public int InfillPercent { get; private set; }
    public int Quantity { get; private set; }
    public bool IncludeDesignService { get; private set; }
    public QuoteRequestStatus Status { get; private set; }

    public static Result<QuoteRequest> Create(
        Guid modelFileId,
        Guid customerUserId,
        Guid materialOptionId,
        QualityPreset qualityPreset,
        decimal layerHeightMm,
        int infillPercent,
        int quantity,
        bool includeDesignService)
    {
        if (quantity < 1) return OrderErrors.InvalidQuantity;
        if (infillPercent is < 0 or > 100) return OrderErrors.InvalidInfill;
        if (layerHeightMm <= 0) return OrderErrors.InvalidLayerHeight;

        return new QuoteRequest(
            Guid.NewGuid(), modelFileId, customerUserId, materialOptionId,
            qualityPreset, layerHeightMm, infillPercent, quantity, includeDesignService);
    }

    /// <summary>Marks the request as Quoted once an operator has produced/sent a quote.</summary>
    public void MarkQuoted()
    {
        Status = QuoteRequestStatus.Quoted;
        Touch();
    }

    public void MarkAccepted()
    {
        Status = QuoteRequestStatus.Accepted;
        Touch();
    }

    public void MarkRejected()
    {
        Status = QuoteRequestStatus.Rejected;
        Touch();
    }

    public void MarkExpired()
    {
        Status = QuoteRequestStatus.Expired;
        Touch();
    }
}
