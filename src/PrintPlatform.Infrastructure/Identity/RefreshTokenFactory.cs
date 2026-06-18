using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using PrintPlatform.Application.Identity.Abstractions;

namespace PrintPlatform.Infrastructure.Identity;

/// <summary>
/// Produces cryptographically-strong opaque refresh tokens and SHA-256 hashes
/// them for at-rest storage.
/// </summary>
public sealed class RefreshTokenFactory : IRefreshTokenFactory
{
    private readonly JwtOptions _options;

    public RefreshTokenFactory(IOptions<JwtOptions> options) => _options = options.Value;

    public (string RawToken, DateTimeOffset ExpiresAt) Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var raw = Convert.ToBase64String(bytes);
        var expires = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays);
        return (raw, expires);
    }

    public string Hash(string rawToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash);
    }
}
