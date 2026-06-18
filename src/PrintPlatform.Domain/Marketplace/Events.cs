using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Marketplace;

/// <summary>Raised when a new printer is registered (status PendingVerification).</summary>
public sealed record PrinterRegisteredEvent(Guid PrinterId, Guid PrinterOwnerProfileId)
    : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>Raised when a printer passes verification and becomes Active.</summary>
public sealed record PrinterVerifiedEvent(Guid PrinterId, Guid PrinterOwnerProfileId, int CertificationLevel)
    : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>Raised when a printer's availability/status changes.</summary>
public sealed record PrinterAvailabilityChangedEvent(Guid PrinterId, PrinterStatus Status)
    : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>Raised when a printer's certification score (level) is updated.</summary>
public sealed record PrinterCertificationUpdatedEvent(Guid PrinterId, int CertificationLevel)
    : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
