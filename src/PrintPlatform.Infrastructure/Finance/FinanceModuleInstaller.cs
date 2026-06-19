using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Infrastructure.Modules;

namespace PrintPlatform.Infrastructure.Finance;

/// <summary>Auto-discovered installer for the Finance module.</summary>
public sealed class FinanceModuleInstaller : IModuleInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
        => services.AddFinanceInfrastructure(configuration);
}
