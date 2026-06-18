using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Loyalty.Abstractions;
using PrintPlatform.Loyalty.Data;
using PrintPlatform.Loyalty.Services;

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

        var connectionString = configuration.GetConnectionString("Loyalty")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (connectionString != null)
        {
            services.AddDbContext<LoyaltyDbContext>(opts =>
                opts.UseNpgsql(connectionString));
        }

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddScoped<ILoyaltyService, LoyaltyService>();
        services.AddSingleton<ITierStrategy, TierStrategy>();

        return services;
    }
}
