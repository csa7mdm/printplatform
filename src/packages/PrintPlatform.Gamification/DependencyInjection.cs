using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Gamification.Extensions;

namespace PrintPlatform.Gamification;

/// <summary>
/// Convenience entry-point that reads configuration from the standard host <see cref="IConfiguration"/>.
/// Prefer <see cref="ServiceCollectionExtensions.AddGamification"/> for full programmatic control.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Gamification services, reading options from
    /// <c>appsettings.json → "Gamification"</c> and the DB connection string
    /// from <c>"ConnectionStrings:Gamification"</c>.
    /// </summary>
    public static IServiceCollection AddGamification(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("Gamification");

        return services.AddGamification(
            configure: opts =>
                configuration.GetSection(GamificationOptions.SectionName).Bind(opts),
            dbContextOptions: connStr is not null
                ? (DbContextOptionsBuilder b) => b.UseNpgsql(connStr)
                : null);
    }
}
