namespace PrintPlatform.Infrastructure.Identity;

/// <summary>Bound from the "Jwt" configuration section.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = "PrintPlatform";
    public string Audience { get; set; } = "PrintPlatform.Clients";

    /// <summary>Access-token lifetime in minutes. Default 15.</summary>
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>Refresh-token lifetime in days. Default 30.</summary>
    public int RefreshTokenDays { get; set; } = 30;
}
