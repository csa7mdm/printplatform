using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Infrastructure.Modules;

namespace PrintPlatform.Infrastructure.Marketplace;

/// <summary>Auto-discovered installer for the Marketplace (supply-side) module.</summary>
public sealed class MarketplaceModuleInstaller : IModuleInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
        => services.AddMarketplaceInfrastructure(configuration);
}
