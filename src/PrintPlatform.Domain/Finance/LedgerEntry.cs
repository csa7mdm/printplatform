using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Finance;

public sealed class LedgerEntry : BaseAggregateRoot<Guid>
{
    private LedgerEntry() { }

    private LedgerEntry(
        LedgerEntryType entryType,
        decimal amountEgp,
        LedgerDirection direction,
        Guid? relatedOrderId,
        Guid? relatedJobAssignmentId,
        Guid? relatedPayoutId,
        Guid? printerOwnerUserId,
        Guid? customerUserId,
        string description,
        string? externalReference,
        Guid? createdByUserId)
    {
        Id = Guid.NewGuid();
        EntryType = entryType;
        AmountEgp = amountEgp;
        Direction = direction;
        RelatedOrderId = relatedOrderId;
        RelatedJobAssignmentId = relatedJobAssignmentId;
        RelatedPayoutId = relatedPayoutId;
        PrinterOwnerUserId = printerOwnerUserId;
        CustomerUserId = customerUserId;
        Description = description;
        ExternalReference = externalReference;
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedByUserId = createdByUserId;

        RaiseDomainEvent(new LedgerEntryRecordedEvent(Id, EntryType, AmountEgp, CreatedAt));
    }

    public LedgerEntryType EntryType { get; private set; }
    public decimal AmountEgp { get; private set; }
    public LedgerDirection Direction { get; private set; }
    public Guid? RelatedOrderId { get; private set; }
    public Guid? RelatedJobAssignmentId { get; private set; }
    public Guid? RelatedPayoutId { get; private set; }
    public Guid? PrinterOwnerUserId { get; private set; }
    public Guid? CustomerUserId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? ExternalReference { get; private set; }
    public new DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    public static Result<LedgerEntry> Create(
        LedgerEntryType entryType,
        decimal amountEgp,
        LedgerDirection direction,
        Guid? relatedOrderId,
        Guid? relatedJobAssignmentId,
        Guid? relatedPayoutId,
        Guid? printerOwnerUserId,
        Guid? customerUserId,
        string description,
        string? externalReference,
        Guid? createdByUserId)
    {
        if (amountEgp <= 0)
            return FinanceErrors.InvalidAmount;

        return new LedgerEntry(
            entryType,
            decimal.Round(amountEgp, 2),
            direction,
            relatedOrderId,
            relatedJobAssignmentId,
            relatedPayoutId,
            printerOwnerUserId,
            customerUserId,
            description,
            externalReference,
            createdByUserId);
    }
}
