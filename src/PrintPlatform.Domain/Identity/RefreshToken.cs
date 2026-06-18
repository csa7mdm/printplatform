namespace PrintPlatform.Domain.Identity;

/// <summary>
/// Value object representing a single refresh token issued to a <see cref="User"/>.
/// The raw token is never stored — only a SHA-256 hash (<see cref="TokenHash"/>).
/// Rotation is supported via <see cref="ReplacedByTokenHash"/>.
/// </summary>
public sealed class RefreshToken
{
    // EF Core ctor.
    private RefreshToken() { }

    private RefreshToken(
        string tokenHash,
        DateTimeOffset expiresAt,
        string createdByIp)
    {
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Surrogate key (owned-entity row id).</summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>SHA-256 hash of the raw refresh token.</summary>
    public string TokenHash { get; private set; } = default!;

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public string CreatedByIp { get; private set; } = default!;

    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }

    /// <summary>Hash of the token that replaced this one when rotated.</summary>
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;

    /// <summary>A token is active when it has neither expired nor been revoked.</summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    public static RefreshToken Create(string tokenHash, DateTimeOffset expiresAt, string createdByIp)
        => new(tokenHash, expiresAt, createdByIp);

    /// <summary>Marks this token as revoked, optionally recording the replacement token hash.</summary>
    public void Revoke(string revokedByIp, string? replacedByTokenHash = null)
    {
        if (IsRevoked)
            return;

        RevokedAt = DateTimeOffset.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
