using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Gamification.Abstractions;
using PrintPlatform.Gamification.Data;
using PrintPlatform.Gamification.Rules;
using PrintPlatform.Gamification.Services;

namespace PrintPlatform.Gamification.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register the Gamification engine.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Gamification services, EF Core context, default achievement rules,
    /// and options into the DI container.
    /// </summary>
    /// <param name="services">The host application's service collection.</param>
    /// <param name="configure">Delegate to customise <see cref="GamificationOptions"/>.</param>
    /// <param name="dbContextOptions">
    /// Delegate to configure the <see cref="GamificationDbContext"/> connection.
    /// Example: <c>o => o.UseNpgsql(connectionString)</c>.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddGamification(
    ///     options => options.BonusMultiplier = 2.0,
    ///     db => db.UseNpgsql(builder.Configuration.GetConnectionString("Gamification")));
    /// </code>
    /// </example>
    public static IServiceCollection AddGamification(
        this IServiceCollection services,
        Action<GamificationOptions>? configure = null,
        Action<DbContextOptionsBuilder>? dbContextOptions = null)
    {
        // ── Options ───────────────────────────────────────────────────────────
        var optBuilder = services.AddOptions<GamificationOptions>()
            .BindConfiguration(GamificationOptions.SectionName);

        if (configure is not null)
            optBuilder.Configure(configure);

        // ── EF Core ───────────────────────────────────────────────────────────
        if (dbContextOptions is not null)
        {
            services.AddDbContext<GamificationDbContext>(dbContextOptions);
        }
        else
        {
            // Fallback: host must configure the connection before calling AddGamification,
            // or override this with UseNpgsql / UseSqlite etc. post-registration.
            services.AddDbContext<GamificationDbContext>();
        }

        // ── MediatR ───────────────────────────────────────────────────────────
        // Register MediatR for this assembly's events.
        // If the host already called AddMediatR, AddMediatR is idempotent per assembly.
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        // ── Core services ─────────────────────────────────────────────────────
        services.AddScoped<AchievementEngine>();
        services.AddScoped<StreakService>();
        services.AddScoped<LeaderboardService>();
        services.AddScoped<IGamificationService, GamificationService>();

        // ── Built-in achievement rules ─────────────────────────────────────────
        services.AddScoped<IAchievementRule, ConsecutiveQCApprovalRule>();
        services.AddScoped<IAchievementRule, SpeedDeliveryRule>();
        services.AddScoped<IAchievementRule, ProfileCompleteRule>();

        // MilestoneJobRule has one instance per milestone ID.
        services.AddScoped<IAchievementRule>(sp =>
            new MilestoneJobRule(MilestoneJobRule.Job1Id,   sp.GetRequiredService<GamificationDbContext>()));
        services.AddScoped<IAchievementRule>(sp =>
            new MilestoneJobRule(MilestoneJobRule.Job10Id,  sp.GetRequiredService<GamificationDbContext>()));
        services.AddScoped<IAchievementRule>(sp =>
            new MilestoneJobRule(MilestoneJobRule.Job50Id,  sp.GetRequiredService<GamificationDbContext>()));
        services.AddScoped<IAchievementRule>(sp =>
            new MilestoneJobRule(MilestoneJobRule.Job100Id, sp.GetRequiredService<GamificationDbContext>()));
        services.AddScoped<IAchievementRule>(sp =>
            new MilestoneJobRule(MilestoneJobRule.Job500Id, sp.GetRequiredService<GamificationDbContext>()));

        return services;
    }
}
