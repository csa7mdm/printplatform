using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Identity.Events;

/// <summary>Raised when a new user successfully registers (customer or printer owner).</summary>
public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string PhoneNumber,
    UserRole Role,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public UserRegisteredEvent(Guid userId, string email, string phoneNumber, UserRole role)
        : this(userId, email, phoneNumber, role, DateTimeOffset.UtcNow) { }
}

/// <summary>Raised when a user's phone number is verified via OTP.</summary>
public sealed record UserVerifiedEvent(
    Guid UserId,
    string PhoneNumber,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public UserVerifiedEvent(Guid userId, string phoneNumber)
        : this(userId, phoneNumber, DateTimeOffset.UtcNow) { }
}

/// <summary>Raised when a printer owner reaches a certified (or higher) level for the first time.</summary>
public sealed record PrinterOwnerCertifiedEvent(
    Guid UserId,
    string BusinessName,
    CertificationLevel Level,
    decimal CertificationScore,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public PrinterOwnerCertifiedEvent(Guid userId, string businessName, CertificationLevel level, decimal score)
        : this(userId, businessName, level, score, DateTimeOffset.UtcNow) { }
}

/// <summary>Raised whenever a printer owner's certification level changes (up or down).</summary>
public sealed record CertificationLevelChangedEvent(
    Guid UserId,
    CertificationLevel OldLevel,
    CertificationLevel NewLevel,
    decimal CertificationScore,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public CertificationLevelChangedEvent(
        Guid userId, CertificationLevel oldLevel, CertificationLevel newLevel, decimal score)
        : this(userId, oldLevel, newLevel, score, DateTimeOffset.UtcNow) { }
}
