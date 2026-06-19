using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Finance;

public sealed record LedgerEntryRecordedEvent(
    Guid LedgerEntryId,
    LedgerEntryType EntryType,
    decimal AmountEgp,
    DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record PayoutCreatedEvent(
    Guid PayoutId,
    Guid PrinterOwnerUserId,
    decimal NetAmountEgp,
    DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record PayoutSentEvent(
    Guid PayoutId,
    Guid PrinterOwnerUserId,
    decimal NetAmountEgp,
    string ExternalReference,
    DateTimeOffset OccurredAt) : IDomainEvent;
