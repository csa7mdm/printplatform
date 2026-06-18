using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Identity.Abstractions;

/// <summary>Issued JWT access token plus the raw refresh token (returned to caller once).</summary>
public sealed record AuthTokens(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

/// <summary>Generates and validates JWT access tokens.</summary>
public interface IJwtTokenGenerator
{
    /// <summary>Creates a signed HS256 access token for the user.</summary>
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user);
}

/// <summary>Generates cryptographically-strong refresh tokens and hashes them for storage.</summary>
public interface IRefreshTokenFactory
{
    /// <summary>Returns a new opaque refresh token (raw) and its expiry.</summary>
    (string RawToken, DateTimeOffset ExpiresAt) Generate();

    /// <summary>Hashes a raw token (SHA-256) for comparison/storage.</summary>
    string Hash(string rawToken);
}

/// <summary>
/// Bridges the Application layer to the ASP.NET Core Identity credential store.
/// Implemented in Infrastructure. Never throws for business outcomes.
/// </summary>
public interface IUserCredentialStore
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task<bool> PhoneExistsAsync(string phoneNumber, CancellationToken ct);

    /// <summary>Creates the ASP.NET Identity credential row and returns the new user id.</summary>
    Task<Result<Guid>> CreateAsync(
        string email, string phoneNumber, string password, UserRole role, CancellationToken ct);

    /// <summary>Validates email + password, returning the user id on success.</summary>
    Task<Result<Guid>> ValidateCredentialsAsync(string email, string password, CancellationToken ct);
}

/// <summary>Sends and verifies one-time passcodes (e.g. via WhatsApp).</summary>
public interface IOtpSender
{
    Task SendAsync(string phoneNumber, Lang lang, CancellationToken ct);
    Task<bool> VerifyAsync(string phoneNumber, string code, CancellationToken ct);
}

/// <summary>Resolves the current authenticated user id from the request context.</summary>
public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
    string? IpAddress { get; }
}

/// <summary>Symmetric encryption for sensitive PII columns (national id, bank details).</summary>
public interface IFieldEncryptor
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}
