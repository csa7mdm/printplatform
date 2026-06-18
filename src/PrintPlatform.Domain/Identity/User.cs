using PrintPlatform.Domain.Identity.Events;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Identity;

/// <summary>
/// Aggregate root for an authenticated platform user. Carries identity, contact,
/// localization and the collection of issued refresh tokens. Credentials
/// (password hashes, lockout, etc.) live in the Infrastructure ASP.NET Identity
/// store and are linked 1:1 by <see cref="Id"/>.
/// </summary>
public sealed class User : BaseAggregateRoot<Guid>
{
    private readonly List<RefreshToken> _refreshTokens = [];

    // EF Core ctor.
    private User() { }

    private User(
        Guid id,
        string email,
        string phoneNumber,
        string fullNameAr,
        string fullNameEn,
        UserRole role,
        Lang lang)
    {
        Id = id;
        Email = email;
        PhoneNumber = phoneNumber;
        FullNameAr = fullNameAr;
        FullNameEn = fullNameEn;
        Role = role;
        Lang = lang;
        IsActive = true;
        IsVerified = false;
    }

    public string Email { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string FullNameAr { get; private set; } = default!;
    public string FullNameEn { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public Lang Lang { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsVerified { get; private set; }

    /// <summary>Optimistic-concurrency token (xmin on PostgreSQL).</summary>
    public uint RowVersion { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    /// <summary>Factory + raises <see cref="UserRegisteredEvent"/>.</summary>
    public static User Register(
        Guid id,
        string email,
        string phoneNumber,
        string fullNameAr,
        string fullNameEn,
        UserRole role,
        Lang lang)
    {
        var user = new User(id, email.Trim().ToLowerInvariant(), phoneNumber.Trim(),
            fullNameAr.Trim(), fullNameEn.Trim(), role, lang);

        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Email, user.PhoneNumber, user.Role));
        return user;
    }

    /// <summary>Marks the phone number as verified. Idempotent-safe via the returned <see cref="Result"/>.</summary>
    public Result MarkPhoneVerified()
    {
        if (IsVerified)
            return Result.Failure(IdentityErrors.AlreadyVerified);

        IsVerified = true;
        Touch();
        RaiseDomainEvent(new UserVerifiedEvent(Id, PhoneNumber));
        return Result.Success();
    }

    public void Deactivate()
    {
        IsActive = false;
        RevokeAllRefreshTokens("system: deactivated");
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void UpdateLanguage(Lang lang)
    {
        Lang = lang;
        Touch();
    }

    // -- Refresh token management -------------------------------------------

    public void AddRefreshToken(RefreshToken token)
    {
        _refreshTokens.Add(token);
        Touch();
    }

    /// <summary>Finds an active refresh token by its hash, or null.</summary>
    public RefreshToken? FindActiveRefreshToken(string tokenHash)
        => _refreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash && t.IsActive);

    public RefreshToken? FindRefreshToken(string tokenHash)
        => _refreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash);

    /// <summary>Rotates an existing token: revokes the old and stores the replacement.</summary>
    public Result RotateRefreshToken(string oldTokenHash, RefreshToken replacement, string ip)
    {
        var existing = FindActiveRefreshToken(oldTokenHash);
        if (existing is null)
            return Result.Failure(IdentityErrors.RefreshTokenExpired);

        existing.Revoke(ip, replacement.TokenHash);
        _refreshTokens.Add(replacement);
        Touch();
        return Result.Success();
    }

    public Result RevokeRefreshToken(string tokenHash, string ip)
    {
        var existing = FindActiveRefreshToken(tokenHash);
        if (existing is null)
            return Result.Failure(IdentityErrors.RefreshTokenExpired);

        existing.Revoke(ip);
        Touch();
        return Result.Success();
    }

    public void RevokeAllRefreshTokens(string ip)
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
            token.Revoke(ip);
        Touch();
    }
}
