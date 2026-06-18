using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Application.Marketplace;
using PrintPlatform.Infrastructure.Data;

namespace PrintPlatform.Infrastructure.Marketplace;

public static class MarketplaceServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Marketplace DbContext, ranking dependencies and placeholder
    /// cross-module providers. Reads <c>ConnectionStrings:Marketplace</c> (falls back
    /// to <c>DefaultConnection</c>).
    /// </summary>
    public static IServiceCollection AddMarketplaceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("Marketplace")
            ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<MarketplaceDbContext>(opts =>
        {
            if (connStr is not null)
                opts.UseNpgsql(connStr, npgsql =>
                    npgsql.MigrationsAssembly(typeof(MarketplaceDbContext).Assembly.FullName));
        });

        // Expose the context through the Application-layer abstraction.
        services.AddScoped<IMarketplaceDbContext>(sp => sp.GetRequiredService<MarketplaceDbContext>());

        // District distance lookup is stateless → singleton.
        services.AddSingleton<IDistrictDistanceCalculator, DistrictDistanceTable>();

        // Cross-module providers — replace with real implementations once
        // the Orders module and live job tracking are available.
        services.AddScoped<IJobRequirementsProvider, NullJobRequirementsProvider>();
        services.AddScoped<IPrinterLoadProvider, NullPrinterLoadProvider>();

        return services;
    }
}
