using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Orders;

/// <summary>Raised when a model file is uploaded to storage and persisted. Triggers geometry analysis.</summary>
public sealed record ModelFileUploadedEvent(
    Guid ModelFileId,
    Guid CustomerUserId,
    string StorageKey,
    FileFormat FileFormat,
    DateTimeOffset OccurredAt) : IDomainEvent;

/// <summary>Raised once background geometry analysis has computed volume / weight / print time.</summary>
public sealed record ModelFileAnalysedEvent(
    Guid ModelFileId,
    decimal EstimatedVolumeCC,
    decimal EstimatedWeightGrams,
    decimal EstimatedPrintHours,
    DateTimeOffset OccurredAt) : IDomainEvent;

/// <summary>Raised when an order transitions into the Confirmed state (payment captured).</summary>
public sealed record OrderConfirmedEvent(
    Guid OrderId,
    Guid CustomerUserId,
    decimal TotalEgp,
    DateTimeOffset OccurredAt) : IDomainEvent;

/// <summary>Raised when an order is cancelled.</summary>
public sealed record OrderCancelledEvent(
    Guid OrderId,
    Guid CustomerUserId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;

/// <summary>Raised when an order is marked delivered.</summary>
public sealed record OrderDeliveredEvent(
    Guid OrderId,
    Guid CustomerUserId,
    DateTimeOffset OccurredAt) : IDomainEvent;
