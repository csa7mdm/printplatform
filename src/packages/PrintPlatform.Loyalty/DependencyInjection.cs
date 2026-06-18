using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PrintPlatform.Loyalty;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all Loyalty services (EF DbContext, MediatR handlers,
    /// tier evaluator, rewards catalog, options) into the DI container.
    /// Call from <c>Program.cs</c>: <c>builder.Services.AddLoyalty(builder.Configuration);</c>
    /// </summary>
    public static IServiceCollection AddLoyalty(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LoyaltyOptions>(
            configuration.GetSection(LoyaltyOptions.SectionName));

        // TODO: services.AddDbContext<LoyaltyDbContext>(...)
        // TODO: services.AddMediatR(Assembly.GetExecutingAssembly())
        // TODO: Register tier evaluator, rewards catalog, referral tracker, etc.

        return services;
    }
}
