using PrintPlatform.Domain.Identity.Events;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Identity;

/// <summary>
/// 1:1 extension of a <see cref="User"/> with <see cref="UserRole.PrinterOwner"/>.
/// Holds KYC (encrypted national id / bank details), business info, payout
/// channels, certification progress and the linked gamification player id.
/// </summary>
public sealed class PrinterOwnerProfile : BaseAggregateRoot<Guid>
{
    private PrinterOwnerProfile() { }

    private PrinterOwnerProfile(Guid userId, string nationalId, string businessName)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        NationalId = nationalId;
        BusinessName = businessName;
        CertificationLevel = CertificationLevel.Pending;
        CertificationScore = 0m;
        IsAvailable = false;
    }

    /// <summary>FK to the owning <see cref="User"/> (1:1).</summary>
    public Guid UserId { get; private set; }

    /// <summary>Egyptian national id — persisted encrypted at the Infrastructure layer.</summary>
    public string NationalId { get; private set; } = default!;

    public string BusinessName { get; private set; } = default!;

    /// <summary>Bank account details — persisted encrypted at the Infrastructure layer.</summary>
    public string? BankAccountDetails { get; private set; }

    /// <summary>InstaPay handle/number for payouts.</summary>
    public string? InstapayNumber { get; private set; }

    public CertificationLevel CertificationLevel { get; private set; }
    public decimal CertificationScore { get; private set; }

    /// <summary>External player id in the reusable Gamification engine.</summary>
    public string? GamificationPlayerId { get; private set; }

    /// <summary>Whether the owner is currently accepting new orders.</summary>
    public bool IsAvailable { get; private set; }

    public DateTimeOffset? OnboardingCompletedAt { get; private set; }

    public uint RowVersion { get; private set; }

    public bool OnboardingCompleted => OnboardingCompletedAt is not null;

    public static PrinterOwnerProfile Create(Guid userId, string nationalId, string businessName)
        => new(userId, nationalId.Trim(), businessName.Trim());

    public void LinkGamificationPlayer(string playerId)
    {
        GamificationPlayerId = playerId;
        Touch();
    }

    /// <summary>Completes onboarding by capturing payout details. Returns failure if already done.</summary>
    public Result CompleteOnboarding(string bankAccountDetails, string? instapayNumber)
    {
        if (OnboardingCompleted)
            return Result.Failure(IdentityErrors.OnboardingAlreadyCompleted);

        BankAccountDetails = bankAccountDetails;
        InstapayNumber = instapayNumber;
        OnboardingCompletedAt = DateTimeOffset.UtcNow;

        // Auto-promote from Pending to Basic once onboarding is complete.
        ApplyCertification(CertificationLevel.Basic, CertificationScore);
        Touch();
        return Result.Success();
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        Touch();
    }

    /// <summary>
    /// Recomputes/raises the certification level based on a new score.
    /// Raises <see cref="CertificationLevelChangedEvent"/> on any change and
    /// <see cref="PrinterOwnerCertifiedEvent"/> on first reaching Certified+.
    /// </summary>
    public void UpdateCertification(decimal newScore)
    {
        var level = LevelForScore(newScore);
        ApplyCertification(level, newScore);
    }

    private void ApplyCertification(CertificationLevel newLevel, decimal score)
    {
        var oldLevel = CertificationLevel;
        CertificationScore = score;

        if (newLevel == oldLevel)
        {
            Touch();
            return;
        }

        var wasCertified = oldLevel >= CertificationLevel.Certified;
        CertificationLevel = newLevel;
        Touch();

        RaiseDomainEvent(new CertificationLevelChangedEvent(UserId, oldLevel, newLevel, score));

        if (!wasCertified && newLevel >= CertificationLevel.Certified)
            RaiseDomainEvent(new PrinterOwnerCertifiedEvent(UserId, BusinessName, newLevel, score));
    }

    private static CertificationLevel LevelForScore(decimal score) => score switch
    {
        >= 90m => CertificationLevel.Elite,
        >= 75m => CertificationLevel.Expert,
        >= 50m => CertificationLevel.Certified,
        >= 25m => CertificationLevel.Basic,
        _ => CertificationLevel.Pending,
    };
}
